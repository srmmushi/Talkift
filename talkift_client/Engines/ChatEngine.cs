using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Talkift.Client.Services;

namespace Talkift.Client.Engines;

public sealed class ChatEngine : IChatEngine
{
    private ClientWebSocket? _socket;
    private CancellationTokenSource? _cts;
    private CancellationTokenSource? _heartbeatCts;
    private Task? _receiveTask;
    private Task? _heartbeatTask;
    private readonly ConcurrentQueue<(string Json, TaskCompletionSource<bool> Tcs)> _sendQueue = new();
    private ChatEngineOptions _options = new();
    private ConnectionState _state = ConnectionState.Disconnected;
    private int _reconnectAttempts;
    private bool _intentionalDisconnect;
    private readonly object _stateLock = new();
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    public ConnectionState State
    {
        get { lock (_stateLock) return _state; }
        private set { lock (_stateLock) _state = value; }
    }

    public bool IsConnected => State == ConnectionState.Connected;

    public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;
    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
    public event EventHandler<MessageSentEventArgs>? MessageSent;
    public event EventHandler<TypingReceivedEventArgs>? TypingReceived;
    public event EventHandler<ConversationUpdatedEventArgs>? ConversationUpdated;
    public event EventHandler<JoinRequestEventArgs>? JoinRequestReceived;
    public event EventHandler<ReactionEventArgs>? ReactionReceived;
    public event EventHandler<ReadReceiptEventArgs>? ReadReceiptReceived;
    public event EventHandler<string>? ErrorOccurred;
    public event EventHandler<string>? RawMessageReceived;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task ConnectAsync(ChatEngineOptions options, CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(_cts?.IsDisposed == true, this);

        _options = options ?? throw new ArgumentNullException(nameof(options));
        _intentionalDisconnect = false;
        _reconnectAttempts = 0;

        await InternalConnectAsync(ct);
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        _intentionalDisconnect = true;
        CancelHeartbeat();
        CancelReceive();

        if (_socket != null)
        {
            try
            {
                if (_socket.State == WebSocketState.Open || _socket.State == WebSocketState.CloseReceived)
                {
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "client disconnect", ct);
                }
            }
            catch { }
            finally
            {
                _socket.Dispose();
                _socket = null;
            }
        }

        SetState(ConnectionState.Disconnected, "client disconnect");
    }

    public async Task SendMessageAsync(string conversationId, string content, CancellationToken ct = default)
    {
        if (!IsConnected)
        {
            FireError("Not connected");
            return;
        }

        var payload = new
        {
            type = "chat",
            payload = new { conversation_id = conversationId, content }
        };

        var json = JsonSerializer.Serialize(payload, JsonOpts);
        var sent = await EnqueueSendAsync(json, ct);
        if (!sent)
        {
            FireError("Send failed or timed out");
        }
    }

    public async Task SendTypingAsync(string conversationId, bool isTyping, CancellationToken ct = default)
    {
        if (!IsConnected) return;

        var payload = new
        {
            type = "typing",
            payload = new { conversation_id = conversationId, is_typing = isTyping }
        };

        var json = JsonSerializer.Serialize(payload, JsonOpts);
        await EnqueueSendAsync(json, ct);
    }

    public async Task SendReactionAsync(string messageId, string conversationId, string emoji, CancellationToken ct = default)
    {
        if (!IsConnected) return;

        var payload = new
        {
            type = "reaction",
            payload = new { message_id = messageId, conversation_id = conversationId, emoji }
        };

        var json = JsonSerializer.Serialize(payload, JsonOpts);
        await EnqueueSendAsync(json, ct);
    }

    public async Task SendReadReceiptAsync(string conversationId, string messageId, CancellationToken ct = default)
    {
        if (!IsConnected) return;

        var payload = new
        {
            type = "read_receipt",
            payload = new { conversation_id = conversationId, message_id = messageId }
        };

        var json = JsonSerializer.Serialize(payload, JsonOpts);
        await EnqueueSendAsync(json, ct);
    }

    public async Task LoadHistoryAsync(string conversationId, string? before = null, CancellationToken ct = default)
    {
        if (!IsConnected) return;

        var payload = new
        {
            type = "load_history",
            payload = new { conversation_id = conversationId, before }
        };

        var json = JsonSerializer.Serialize(payload, JsonOpts);
        await EnqueueSendAsync(json, ct);
    }

    public async Task LoadConversationsAsync(CancellationToken ct = default)
    {
        if (!IsConnected) return;

        var payload = new { type = "load_conversations", payload = new { } };
        var json = JsonSerializer.Serialize(payload, JsonOpts);
        await EnqueueSendAsync(json, ct);
    }

    public async Task CreateGroupAsync(string name, List<string> members, CancellationToken ct = default)
    {
        if (!IsConnected) return;

        var payload = new
        {
            type = "create_group",
            payload = new { name, members }
        };

        var json = JsonSerializer.Serialize(payload, JsonOpts);
        await EnqueueSendAsync(json, ct);
    }

    public async Task CreateConversationAsync(string name, string targetUsername, CancellationToken ct = default)
    {
        if (!IsConnected) return;

        var payload = new
        {
            type = "create_conversation",
            payload = new { name, target_username = targetUsername }
        };

        var json = JsonSerializer.Serialize(payload, JsonOpts);
        await EnqueueSendAsync(json, ct);
    }

    public async Task SendMuteAsync(string groupId, string targetUsername, CancellationToken ct = default)
    {
        if (!IsConnected) return;

        var payload = new
        {
            type = "mute",
            payload = new { group_id = groupId, target_username = targetUsername }
        };

        var json = JsonSerializer.Serialize(payload, JsonOpts);
        await EnqueueSendAsync(json, ct);
    }

    public async Task SendLeaveGroupAsync(string groupId, CancellationToken ct = default)
    {
        if (!IsConnected) return;

        var payload = new
        {
            type = "leave_group",
            payload = new { group_id = groupId }
        };

        var json = JsonSerializer.Serialize(payload, JsonOpts);
        await EnqueueSendAsync(json, ct);
    }

    public async Task SendDndAsync(string conversationId, bool muted, CancellationToken ct = default)
    {
        if (!IsConnected) return;

        var payload = new
        {
            type = "dnd",
            payload = new { conversation_id = conversationId, muted }
        };

        var json = JsonSerializer.Serialize(payload, JsonOpts);
        await EnqueueSendAsync(json, ct);
    }

    public async Task PinMessageAsync(string messageId, string conversationId, CancellationToken ct = default)
    {
        if (!IsConnected) return;

        var payload = new
        {
            type = "pin_message",
            payload = new { message_id = messageId, conversation_id = conversationId }
        };

        var json = JsonSerializer.Serialize(payload, JsonOpts);
        await EnqueueSendAsync(json, ct);
    }

    public async Task EditMessageAsync(string messageId, string conversationId, string newContent, CancellationToken ct = default)
    {
        if (!IsConnected) return;

        var payload = new
        {
            type = "edit_message",
            payload = new { message_id = messageId, conversation_id = conversationId, content = newContent }
        };

        var json = JsonSerializer.Serialize(payload, JsonOpts);
        await EnqueueSendAsync(json, ct);
    }

    public async Task DeleteMessageAsync(string messageId, string conversationId, CancellationToken ct = default)
    {
        if (!IsConnected) return;

        var payload = new
        {
            type = "delete_message",
            payload = new { message_id = messageId, conversation_id = conversationId }
        };

        var json = JsonSerializer.Serialize(payload, JsonOpts);
        await EnqueueSendAsync(json, ct);
    }

    private async Task InternalConnectAsync(CancellationToken ct)
    {
        SetState(ConnectionState.Connecting, "connecting");

        try
        {
            CancelReceive();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, ct);

            _socket?.Dispose();
            _socket = new ClientWebSocket();
            _socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(15);

            var url = _options.WebSocketUrl;
            if (!string.IsNullOrEmpty(_options.Token))
            {
                _socket.Options.SetRequestHeader("Authorization", $"Bearer {_options.Token}");
            }
            _socket.Options.SetRequestHeader("X-User-Id", _options.UserId);

            await _socket.ConnectAsync(new Uri(url), linkedCts.Token);

            SetState(ConnectionState.Connected, "connected");
            _reconnectAttempts = 0;

            _receiveTask = Task.Run(() => ReceiveLoopAsync(_cts.Token), _cts.Token);
            StartHeartbeat(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            if (!_intentionalDisconnect)
                SetState(ConnectionState.Disconnected, "connect cancelled");
        }
        catch (Exception ex)
        {
            FireError($"Connect failed: {ex.Message}");
            SetState(ConnectionState.Failed, ex.Message);
            _ = AttemptReconnectAsync();
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        var buffer = new byte[8192];
        var sb = new StringBuilder();

        try
        {
            while (!ct.IsCancellationRequested && _socket?.State == WebSocketState.Open)
            {
                sb.Clear();
                WebSocketReceiveResult result;
                do
                {
                    result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        return;
                    }
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                } while (!result.EndOfMessage);

                var raw = sb.ToString();
                RawMessageReceived?.Invoke(this, raw);

                try
                {
                    DispatchMessage(raw);
                }
                catch (Exception ex)
                {
                    CrashLogger.LogException("ChatEngine.DispatchMessage", ex);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (WebSocketException)
        {
            if (!_intentionalDisconnect)
            {
                SetState(ConnectionState.Disconnected, "connection lost");
                _ = AttemptReconnectAsync();
            }
        }
        catch (Exception ex)
        {
            FireError($"Receive error: {ex.Message}");
            if (!_intentionalDisconnect)
            {
                SetState(ConnectionState.Disconnected, ex.Message);
                _ = AttemptReconnectAsync();
            }
        }
    }

    private void DispatchMessage(string raw)
    {
        using var doc = JsonDocument.Parse(raw);
        var root = doc.RootElement;

        if (!root.TryGetProperty("type", out var typeProp))
            return;

        var type = typeProp.GetString() ?? "";
        var payload = root.TryGetProperty("payload", out var p) ? p : default;

        switch (type)
        {
            case "chat":
                DispatchChatMessage(payload);
                break;
            case "history":
                DispatchHistory(payload);
                break;
            case "conversations":
                DispatchConversations(payload);
                break;
            case "typing":
                DispatchTyping(payload);
                break;
            case "conversation_updated":
                DispatchConversationUpdated(payload);
                break;
            case "join_request":
                DispatchJoinRequest(payload);
                break;
            case "reaction":
                DispatchReaction(payload);
                break;
            case "read_receipt":
                DispatchReadReceipt(payload);
                break;
            case "error":
                var errMsg = payload.TryGetProperty("message", out var mp) ? mp.GetString() ?? "unknown" : "unknown";
                FireError(errMsg);
                break;
        }
    }

    private void DispatchChatMessage(JsonElement payload)
    {
        var ev = new MessageReceivedEventArgs
        {
            MessageId = payload.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "",
            ConversationId = payload.TryGetProperty("conversation_id", out var cid) ? cid.GetString() ?? "" : "",
            SenderId = payload.TryGetProperty("sender_id", out var sid) ? sid.GetString() ?? "" : "",
            SenderName = payload.TryGetProperty("sender_name", out var sn) ? sn.GetString() ?? "" : "",
            Content = payload.TryGetProperty("content", out var c) ? c.GetString() ?? "" : "",
            Timestamp = payload.TryGetProperty("timestamp", out var ts) ? ts.GetInt64() : DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Type = payload.TryGetProperty("type", out var t) ? t.GetString() ?? "chat" : "chat"
        };
        MessageReceived?.Invoke(this, ev);
    }

    private void DispatchHistory(JsonElement payload)
    {
        if (!payload.TryGetProperty("messages", out var arr)) return;
        foreach (var item in arr.EnumerateArray())
        {
            DispatchChatMessage(item);
        }
    }

    private void DispatchConversations(JsonElement payload)
    {
        if (!payload.TryGetProperty("conversations", out var arr)) return;
        foreach (var item in arr.EnumerateArray())
        {
            var ev = new ConversationUpdatedEventArgs
            {
                ConversationId = item.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "",
                Name = item.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
                IsGroup = item.TryGetProperty("is_group", out var ig) && ig.GetBoolean(),
                LastMessage = item.TryGetProperty("last_message", out var lm) ? lm.GetString() : null,
                LastMessageTimestamp = item.TryGetProperty("last_message_timestamp", out var lmt) ? lmt.GetInt64() : null
            };

            if (item.TryGetProperty("members", out var mem))
            {
                foreach (var m in mem.EnumerateArray())
                    ev.Members.Add(m.GetString() ?? "");
            }

            ConversationUpdated?.Invoke(this, ev);
        }
    }

    private void DispatchTyping(JsonElement payload)
    {
        var ev = new TypingReceivedEventArgs
        {
            ConversationId = payload.TryGetProperty("conversation_id", out var cid) ? cid.GetString() ?? "" : "",
            UserId = payload.TryGetProperty("user_id", out var uid) ? uid.GetString() ?? "" : "",
            Username = payload.TryGetProperty("username", out var un) ? un.GetString() ?? "" : "",
            IsTyping = payload.TryGetProperty("is_typing", out var it) && it.GetBoolean()
        };
        TypingReceived?.Invoke(this, ev);
    }

    private void DispatchConversationUpdated(JsonElement payload)
    {
        var ev = new ConversationUpdatedEventArgs
        {
            ConversationId = payload.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "",
            Name = payload.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
            IsGroup = payload.TryGetProperty("is_group", out var ig) && ig.GetBoolean()
        };

        if (payload.TryGetProperty("members", out var mem))
        {
            foreach (var m in mem.EnumerateArray())
                ev.Members.Add(m.GetString() ?? "");
        }

        ConversationUpdated?.Invoke(this, ev);
    }

    private void DispatchJoinRequest(JsonElement payload)
    {
        var ev = new JoinRequestEventArgs
        {
            FromUserId = payload.TryGetProperty("from_user_id", out var uid) ? uid.GetString() ?? "" : "",
            FromUsername = payload.TryGetProperty("from_username", out var un) ? un.GetString() ?? "" : "",
            GroupId = payload.TryGetProperty("group_id", out var gid) ? gid.GetString() ?? "" : "",
            GroupName = payload.TryGetProperty("group_name", out var gn) ? gn.GetString() ?? "" : ""
        };
        JoinRequestReceived?.Invoke(this, ev);
    }

    private void DispatchReaction(JsonElement payload)
    {
        var ev = new ReactionEventArgs
        {
            MessageId = payload.TryGetProperty("message_id", out var mid) ? mid.GetString() ?? "" : "",
            ConversationId = payload.TryGetProperty("conversation_id", out var cid) ? cid.GetString() ?? "" : "",
            Emoji = payload.TryGetProperty("emoji", out var e) ? e.GetString() ?? "" : "",
            UserId = payload.TryGetProperty("user_id", out var uid) ? uid.GetString() ?? "" : ""
        };
        ReactionReceived?.Invoke(this, ev);
    }

    private void DispatchReadReceipt(JsonElement payload)
    {
        var ev = new ReadReceiptEventArgs
        {
            ConversationId = payload.TryGetProperty("conversation_id", out var cid) ? cid.GetString() ?? "" : "",
            MessageId = payload.TryGetProperty("message_id", out var mid) ? mid.GetString() ?? "" : "",
            UserId = payload.TryGetProperty("user_id", out var uid) ? uid.GetString() ?? "" : "",
            Timestamp = payload.TryGetProperty("timestamp", out var ts) ? ts.GetInt64() : DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        ReadReceiptReceived?.Invoke(this, ev);
    }

    private async Task<bool> EnqueueSendAsync(string json, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _sendQueue.Enqueue((json, tcs));

        _ = Task.Run(async () =>
        {
            await _sendLock.WaitAsync(ct);
            try
            {
                while (_sendQueue.TryDequeue(out var item))
                {
                    if (_socket?.State != WebSocketState.Open)
                    {
                        item.Tcs.TrySetResult(false);
                        continue;
                    }

                    try
                    {
                        var bytes = Encoding.UTF8.GetBytes(item.Json);
                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                        cts.CancelAfter(_options.SendTimeout);
                        await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cts.Token);
                        item.Tcs.TrySetResult(true);
                    }
                    catch
                    {
                        item.Tcs.TrySetResult(false);
                    }
                }
            }
            finally
            {
                _sendLock.Release();
            }
        }, ct);

        return await tcs.Task.WaitAsync(_options.SendTimeout, ct).ConfigureAwait(false);
    }

    private void StartHeartbeat(CancellationToken ct)
    {
        CancelHeartbeat();
        _heartbeatCts = new CancellationTokenSource();
        var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, _heartbeatCts.Token);

        _heartbeatTask = Task.Run(async () =>
        {
            while (!linked.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_options.HeartbeatInterval, linked.Token);
                    if (IsConnected)
                    {
                        var json = JsonSerializer.Serialize(new { type = "ping", payload = new { } }, JsonOpts);
                        await EnqueueSendAsync(json, linked.Token);
                    }
                }
                catch (OperationCanceledException) { break; }
                catch { break; }
            }
        }, linked.Token);
    }

    private void CancelHeartbeat()
    {
        _heartbeatCts?.Cancel();
        _heartbeatCts?.Dispose();
        _heartbeatCts = null;
    }

    private void CancelReceive()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private async Task AttemptReconnectAsync()
    {
        if (_intentionalDisconnect) return;
        if (_reconnectAttempts >= _options.MaxReconnectAttempts)
        {
            FireError("Max reconnect attempts reached");
            SetState(ConnectionState.Failed, "max attempts");
            return;
        }

        SetState(ConnectionState.Reconnecting, $"attempt {_reconnectAttempts + 1}");
        _reconnectAttempts++;

        var delay = TimeSpan.FromTicks(
            Math.Min(_options.ReconnectDelay.Ticks * (1L << Math.Min(_reconnectAttempts - 1, 5)),
                     _options.MaxReconnectDelay.Ticks));

        try
        {
            await Task.Delay(delay);
            if (!_intentionalDisconnect)
                await InternalConnectAsync(CancellationToken.None);
        }
        catch { }
    }

    private void SetState(ConnectionState newState, string? reason)
    {
        var old = State;
        if (old == newState) return;
        State = newState;
        ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs
        {
            OldState = old,
            NewState = newState,
            Reason = reason
        });
    }

    private void FireError(string message)
    {
        ErrorOccurred?.Invoke(this, message);
    }

    public void Dispose()
    {
        _intentionalDisconnect = true;
        CancelHeartbeat();
        CancelReceive();

        try { _socket?.Dispose(); } catch { }
        _socket = null;

        _sendLock.Dispose();
        GC.SuppressFinalize(this);
    }
}
