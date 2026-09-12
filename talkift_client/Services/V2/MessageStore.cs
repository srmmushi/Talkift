using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Talkift.Client.Services.V2;

public sealed class MessageStore : IMessageStore
{
    private readonly string _storeDir;
    private readonly Dictionary<string, List<MessageStoreEntry>> _cache = new();
    private readonly object _lock = new();
    private bool _disposed;

    public MessageStore()
    {
        _storeDir = Path.Combine(StorageService.DataDir, "messages");
        Directory.CreateDirectory(_storeDir);
    }

    public Task AddMessageAsync(string conversationId, MessageStoreEntry message, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (!_cache.TryGetValue(conversationId, out var list))
            {
                list = LoadFromFile(conversationId);
                _cache[conversationId] = list;
            }

            if (!list.Any(m => m.MessageId == message.MessageId))
            {
                list.Add(message);
            }
        }

        _ = SaveToFileAsync(conversationId);
        return Task.CompletedTask;
    }

    public Task<List<MessageStoreEntry>> GetMessagesAsync(string conversationId, int limit = 50, string? before = null, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (!_cache.TryGetValue(conversationId, out var list))
            {
                list = LoadFromFile(conversationId);
                _cache[conversationId] = list;
            }

            IEnumerable<MessageStoreEntry> query = list.OrderByDescending(m => m.Timestamp);

            if (!string.IsNullOrEmpty(before))
            {
                var beforeMsg = list.FirstOrDefault(m => m.MessageId == before);
                if (beforeMsg != null)
                {
                    query = query.Where(m => m.Timestamp < beforeMsg.Timestamp);
                }
            }

            return Task.FromResult(query.Take(limit).Reverse().ToList());
        }
    }

    public Task<MessageStoreEntry?> GetMessageAsync(string messageId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            foreach (var list in _cache.Values)
            {
                var msg = list.FirstOrDefault(m => m.MessageId == messageId);
                if (msg != null) return Task.FromResult<MessageStoreEntry?>(msg);
            }
        }
        return Task.FromResult<MessageStoreEntry?>(null);
    }

    public Task UpdateMessageAsync(MessageStoreEntry message, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(message.ConversationId, out var list))
            {
                var idx = list.FindIndex(m => m.MessageId == message.MessageId);
                if (idx >= 0)
                {
                    list[idx] = message;
                    _ = SaveToFileAsync(message.ConversationId);
                }
            }
        }
        return Task.CompletedTask;
    }

    public Task DeleteMessageAsync(string messageId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            foreach (var kvp in _cache)
            {
                var idx = kvp.Value.FindIndex(m => m.MessageId == messageId);
                if (idx >= 0)
                {
                    kvp.Value[idx].IsDeleted = true;
                    _ = SaveToFileAsync(kvp.Key);
                    break;
                }
            }
        }
        return Task.CompletedTask;
    }

    public Task PinMessageAsync(string messageId, bool pinned, CancellationToken ct = default)
    {
        lock (_lock)
        {
            foreach (var kvp in _cache)
            {
                var msg = kvp.Value.FirstOrDefault(m => m.MessageId == messageId);
                if (msg != null)
                {
                    msg.IsPinned = pinned;
                    _ = SaveToFileAsync(kvp.Key);
                    break;
                }
            }
        }
        return Task.CompletedTask;
    }

    public Task AddReactionAsync(string messageId, string emoji, CancellationToken ct = default)
    {
        lock (_lock)
        {
            foreach (var kvp in _cache)
            {
                var msg = kvp.Value.FirstOrDefault(m => m.MessageId == messageId);
                if (msg != null && !msg.Reactions.Contains(emoji))
                {
                    msg.Reactions.Add(emoji);
                    _ = SaveToFileAsync(kvp.Key);
                    break;
                }
            }
        }
        return Task.CompletedTask;
    }

    public Task RemoveReactionAsync(string messageId, string emoji, CancellationToken ct = default)
    {
        lock (_lock)
        {
            foreach (var kvp in _cache)
            {
                var msg = kvp.Value.FirstOrDefault(m => m.MessageId == messageId);
                if (msg != null)
                {
                    msg.Reactions.Remove(emoji);
                    _ = SaveToFileAsync(kvp.Key);
                    break;
                }
            }
        }
        return Task.CompletedTask;
    }

    public Task ClearAsync(string conversationId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _cache.Remove(conversationId);
        }

        var filePath = GetFilePath(conversationId);
        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    private string GetFilePath(string conversationId)
    {
        var safeName = string.Join("_", conversationId.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_storeDir, $"{safeName}.json");
    }

    private List<MessageStoreEntry> LoadFromFile(string conversationId)
    {
        try
        {
            var path = GetFilePath(conversationId);
            if (!File.Exists(path)) return new List<MessageStoreEntry>();

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<MessageStoreEntry>>(json) ?? new List<MessageStoreEntry>();
        }
        catch
        {
            return new List<MessageStoreEntry>();
        }
    }

    private async Task SaveToFileAsync(string conversationId)
    {
        try
        {
            List<MessageStoreEntry> list;
            lock (_lock)
            {
                if (!_cache.TryGetValue(conversationId, out list!))
                    return;
            }

            var path = GetFilePath(conversationId);
            var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, json);
        }
        catch { }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
