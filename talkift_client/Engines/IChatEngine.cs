using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Talkift.Client.Engines;

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting,
    Failed
}

public sealed class ConnectionStateChangedEventArgs : EventArgs
{
    public ConnectionState OldState { get; init; }
    public ConnectionState NewState { get; init; }
    public string? Reason { get; init; }
}

public sealed class MessageReceivedEventArgs : EventArgs
{
    public string ConversationId { get; init; } = string.Empty;
    public string SenderId { get; init; } = string.Empty;
    public string SenderName { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public long Timestamp { get; init; }
    public string Type { get; init; } = "chat";
    public string MessageId { get; init; } = string.Empty;
}

public sealed class MessageSentEventArgs : EventArgs
{
    public string MessageId { get; init; } = string.Empty;
    public string ConversationId { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string? Error { get; init; }
}

public sealed class TypingReceivedEventArgs : EventArgs
{
    public string ConversationId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public bool IsTyping { get; init; }
}

public sealed class ConversationUpdatedEventArgs : EventArgs
{
    public string ConversationId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsGroup { get; init; }
    public List<string> Members { get; init; } = new();
    public string? LastMessage { get; init; }
    public long? LastMessageTimestamp { get; init; }
}

public sealed class JoinRequestEventArgs : EventArgs
{
    public string FromUserId { get; init; } = string.Empty;
    public string FromUsername { get; init; } = string.Empty;
    public string GroupId { get; init; } = string.Empty;
    public string GroupName { get; init; } = string.Empty;
}

public sealed class ReactionEventArgs : EventArgs
{
    public string MessageId { get; init; } = string.Empty;
    public string ConversationId { get; init; } = string.Empty;
    public string Emoji { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
}

public sealed class ReadReceiptEventArgs : EventArgs
{
    public string ConversationId { get; init; } = string.Empty;
    public string MessageId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public long Timestamp { get; init; }
}

public interface IChatEngine : IDisposable
{
    ConnectionState State { get; }
    bool IsConnected { get; }

    event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;
    event EventHandler<MessageReceivedEventArgs>? MessageReceived;
    event EventHandler<MessageSentEventArgs>? MessageSent;
    event EventHandler<TypingReceivedEventArgs>? TypingReceived;
    event EventHandler<ConversationUpdatedEventArgs>? ConversationUpdated;
    event EventHandler<JoinRequestEventArgs>? JoinRequestReceived;
    event EventHandler<ReactionEventArgs>? ReactionReceived;
    event EventHandler<ReadReceiptEventArgs>? ReadReceiptReceived;
    event EventHandler<string>? ErrorOccurred;
    event EventHandler<string>? RawMessageReceived;

    Task ConnectAsync(ChatEngineOptions options, CancellationToken ct = default);
    Task DisconnectAsync(CancellationToken ct = default);
    Task SendMessageAsync(string conversationId, string content, CancellationToken ct = default);
    Task SendTypingAsync(string conversationId, bool isTyping, CancellationToken ct = default);
    Task SendReactionAsync(string messageId, string conversationId, string emoji, CancellationToken ct = default);
    Task SendReadReceiptAsync(string conversationId, string messageId, CancellationToken ct = default);
    Task LoadHistoryAsync(string conversationId, string? before = null, CancellationToken ct = default);
    Task LoadConversationsAsync(CancellationToken ct = default);
    Task CreateGroupAsync(string name, List<string> members, CancellationToken ct = default);
    Task CreateConversationAsync(string name, string targetUsername, CancellationToken ct = default);
    Task SendMuteAsync(string groupId, string targetUsername, CancellationToken ct = default);
    Task SendLeaveGroupAsync(string groupId, CancellationToken ct = default);
    Task SendDndAsync(string conversationId, bool muted, CancellationToken ct = default);
    Task PinMessageAsync(string messageId, string conversationId, CancellationToken ct = default);
    Task EditMessageAsync(string messageId, string conversationId, string newContent, CancellationToken ct = default);
    Task DeleteMessageAsync(string messageId, string conversationId, CancellationToken ct = default);
}
