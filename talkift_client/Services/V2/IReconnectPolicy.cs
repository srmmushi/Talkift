using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Talkift.Client.Services;

public interface IReconnectPolicy
{
    TimeSpan GetDelay(int attemptNumber);
    bool ShouldRetry(int attemptNumber);
    void Reset();
}

public sealed class ExponentialBackoffPolicy : IReconnectPolicy
{
    private readonly TimeSpan _baseDelay;
    private readonly TimeSpan _maxDelay;
    private readonly int _maxAttempts;
    private readonly Random _jitter = new();

    public ExponentialBackoffPolicy(
        TimeSpan? baseDelay = null,
        TimeSpan? maxDelay = null,
        int maxAttempts = 10)
    {
        _baseDelay = baseDelay ?? TimeSpan.FromSeconds(2);
        _maxDelay = maxDelay ?? TimeSpan.FromSeconds(30);
        _maxAttempts = maxAttempts;
    }

    public TimeSpan GetDelay(int attemptNumber)
    {
        var exponential = _baseDelay.Ticks * (1L << Math.Min(attemptNumber, 6));
        var capped = Math.Min(exponential, _maxDelay.Ticks);
        var jitterMs = _jitter.Next(0, (int)(capped / TimeSpan.TicksPerMillisecond / 4));
        return TimeSpan.FromTicks(capped) + TimeSpan.FromMilliseconds(jitterMs);
    }

    public bool ShouldRetry(int attemptNumber) => attemptNumber < _maxAttempts;

    public void Reset() { }
}

public sealed class MessageStoreEntry
{
    public string MessageId { get; init; } = string.Empty;
    public string ConversationId { get; init; } = string.Empty;
    public string SenderId { get; init; } = string.Empty;
    public string SenderName { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public long Timestamp { get; init; }
    public string Type { get; init; } = "chat";
    public bool IsMine { get; set; }
    public string TimeDisplay { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public List<string> Reactions { get; init; } = new();
    public string? ReplyToMessageId { get; init; }
    public bool IsDeleted { get; set; }
    public bool IsEdited { get; set; }
}

public interface IMessageStore : IDisposable
{
    Task AddMessageAsync(string conversationId, MessageStoreEntry message, CancellationToken ct = default);
    Task<List<MessageStoreEntry>> GetMessagesAsync(string conversationId, int limit = 50, string? before = null, CancellationToken ct = default);
    Task<MessageStoreEntry?> GetMessageAsync(string messageId, CancellationToken ct = default);
    Task UpdateMessageAsync(MessageStoreEntry message, CancellationToken ct = default);
    Task DeleteMessageAsync(string messageId, CancellationToken ct = default);
    Task PinMessageAsync(string messageId, bool pinned, CancellationToken ct = default);
    Task AddReactionAsync(string messageId, string emoji, CancellationToken ct = default);
    Task RemoveReactionAsync(string messageId, string emoji, CancellationToken ct = default);
    Task ClearAsync(string conversationId, CancellationToken ct = default);
}
