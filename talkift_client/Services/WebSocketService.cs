using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Talkift.Client.Models;

namespace Talkift.Client.Services
{
    public class WebSocketService : IDisposable
    {
        private ClientWebSocket? _webSocket;
        private CancellationTokenSource? _cts;
        private string? _url;
        private int _reconnectDelay = 1000;
        private const int MaxReconnectDelay = 30000;
        private Timer? _heartbeatTimer;
        private bool _shouldReconnect;
        private string? _userId;

        public event Action<string>? MessageReceived;
        public event Action<string, ChatMessage>? NewMessageReceived;
        public event Action<Conversation>? ConversationUpdated;
        public event Action<string>? UserJoined;
        public event Action? Connected;
        public event Action? Disconnected;
        public event Action<string>? ErrorOccurred;
        public event Action<string, string>? JoinRequestReceived;

        public bool IsConnected => _webSocket?.State == WebSocketState.Open;
        public string? UserId => _userId;

        public async Task ConnectAsync(string url, string? token = null, string? userId = null)
        {
            _url = url;
            _userId = userId;
            _shouldReconnect = true;

            try
            {
                _webSocket = new ClientWebSocket();
                _cts = new CancellationTokenSource();

                if (!string.IsNullOrEmpty(token))
                    _webSocket.Options.SetRequestHeader("Authorization", $"Bearer {token}");

                await _webSocket.ConnectAsync(new Uri(url), _cts.Token);
                _reconnectDelay = 1000;
                Connected?.Invoke();

                StartHeartbeat();
                _ = ReceiveLoopAsync();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Connection failed: {ex.Message}");
                Disconnected?.Invoke();
            }
        }

        public async Task SendAsync(string message)
        {
            if (_webSocket?.State != WebSocketState.Open)
                return;

            var bytes = Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text, true, _cts?.Token ?? CancellationToken.None);
        }

        public async Task SendAsync(object obj)
        {
            var json = JsonSerializer.Serialize(obj);
            await SendAsync(json);
        }

        public async Task SendChatAsync(string conversationId, string content)
        {
            await SendAsync(new { type = "chat", payload = new ChatPayload { ConversationId = conversationId, Content = content } });
        }

        public async Task SendCreateGroupAsync(string name, System.Collections.Generic.List<string> members)
        {
            await SendAsync(new { type = "create_group", payload = new CreateGroupPayload { Name = name, Members = members } });
        }

        public async Task SendCreateConversationAsync(string name, string targetUsername)
        {
            await SendAsync(new { type = "create_conversation", payload = new CreateConversationPayload { Name = name, TargetUsername = targetUsername } });
        }

        public async Task SendLoadHistoryAsync(string conversationId, string before = "")
        {
            await SendAsync(new { type = "load_history", payload = new LoadHistoryPayload { ConversationId = conversationId, Before = before } });
        }

        public async Task SendDndAsync(string conversationId, bool muted)
        {
            await SendAsync(new { type = "dnd", payload = new DndPayload { ConversationId = conversationId, Muted = muted } });
        }

        public async Task SendMuteAsync(string groupId, string targetUsername)
        {
            await SendAsync(new { type = "mute", payload = new MutePayload { GroupId = groupId, TargetUsername = targetUsername } });
        }

        public async Task SendLeaveGroupAsync(string groupId)
        {
            await SendAsync(new { type = "leave", payload = new LeaveGroupPayload { GroupId = groupId } });
        }

        public async Task SendLoadConversationsAsync()
        {
            await SendAsync(new { type = "load_conversations" });
        }

        public async Task SendLoadMembersAsync(string groupId)
        {
            await SendAsync(new { type = "load_members", group_id = groupId });
        }

        private async Task ReceiveLoopAsync()
        {
            var buffer = new byte[8192];

            try
            {
                while (_webSocket?.State == WebSocketState.Open)
                {
                    var result = await _webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), _cts?.Token ?? CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _webSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        OnDisconnected();
                    }
                    else
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        HandleMessage(message);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception)
            {
                OnDisconnected();
            }
        }

        private void HandleMessage(string rawMessage)
        {
            try
            {
                using var doc = JsonDocument.Parse(rawMessage);
                var root = doc.RootElement;
                var type = root.GetProperty("type").GetString() ?? "";

                MessageReceived?.Invoke(rawMessage);

                switch (type)
                {
                    case "chat":
                        if (root.TryGetProperty("payload", out var payload))
                        {
                            var msg = JsonSerializer.Deserialize<ChatMessage>(payload.GetRawText());
                            if (msg != null)
                            {
                                var senderId = msg.SenderId;
                                msg.IsMine = senderId == _userId;
                                msg.TimeDisplay = FormatTimestamp(msg.Timestamp);
                                NewMessageReceived?.Invoke(msg.ConversationId, msg);
                            }
                        }
                        break;

                    case "conversation_created":
                    case "conversation_updated":
                        if (root.TryGetProperty("payload", out var convPayload))
                        {
                            var conv = JsonSerializer.Deserialize<Conversation>(convPayload.GetRawText());
                            if (conv != null)
                                ConversationUpdated?.Invoke(conv);
                        }
                        break;

                    case "user_joined":
                        if (root.TryGetProperty("payload", out var joinPayload))
                        {
                            var username = joinPayload.GetProperty("username").GetString() ?? "";
                            UserJoined?.Invoke(username);
                        }
                        break;

                    case "join_request":
                        if (root.TryGetProperty("payload", out var jrPayload))
                        {
                            var groupName = jrPayload.TryGetProperty("group_name", out var gn) ? gn.GetString() ?? "" : "";
                            var fromUser = jrPayload.TryGetProperty("from_user", out var fu) ? fu.GetString() ?? "" : "";
                            JoinRequestReceived?.Invoke(fromUser, groupName);
                        }
                        break;

                    case "error":
                        if (root.TryGetProperty("payload", out var errPayload))
                        {
                            var errMsg = errPayload.TryGetProperty("message", out var em) ? em.GetString() ?? "" : "Unknown error";
                            ErrorOccurred?.Invoke(errMsg);
                        }
                        break;

                    case "pong":
                        break;
                }
            }
            catch (JsonException)
            {
            }
        }

        private void StartHeartbeat()
        {
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = new Timer(async _ =>
            {
                if (IsConnected)
                {
                    try
                    {
                        await SendAsync(new { type = "ping" });
                    }
                    catch
                    {
                    }
                }
            }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        private async void OnDisconnected()
        {
            _heartbeatTimer?.Dispose();
            Disconnected?.Invoke();

            if (_shouldReconnect && _url != null)
            {
                await Task.Delay(_reconnectDelay);
                _reconnectDelay = Math.Min(_reconnectDelay * 2, MaxReconnectDelay);

                if (_shouldReconnect)
                {
                    try
                    {
                        await ConnectAsync(_url, userId: _userId);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public async Task DisconnectAsync()
        {
            _shouldReconnect = false;
            _heartbeatTimer?.Dispose();

            if (_webSocket?.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            _cts?.Cancel();
        }

        private static string FormatTimestamp(long timestamp)
        {
            var dt = DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;
            var now = DateTime.Now;
            if (dt.Date == now.Date)
                return dt.ToString("HH:mm");
            if (dt.Date == now.Date.AddDays(-1))
                return "Yesterday " + dt.ToString("HH:mm");
            return dt.ToString("MM/dd HH:mm");
        }

        public void Dispose()
        {
            _shouldReconnect = false;
            _heartbeatTimer?.Dispose();
            _cts?.Cancel();
            _cts?.Dispose();
            _webSocket?.Dispose();
        }
    }
}
