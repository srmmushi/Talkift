using Talkift.Client.Models.V2;
using Talkift.Client.Services;

namespace Talkift.Client.Helpers;

public static class ChatMessageMapper
{
    public static ChatMessageModel ToChatMessage(this MessageStoreEntry entry) => new()
    {
        Id = entry.MessageId,
        SenderId = entry.SenderId,
        SenderName = entry.SenderName,
        ConversationId = entry.ConversationId,
        Content = entry.Content,
        Timestamp = entry.Timestamp,
        Type = entry.Type,
        IsPinned = entry.IsPinned,
        ReplyToMessageId = entry.ReplyToMessageId,
        IsEdited = entry.IsEdited,
        IsDeleted = entry.IsDeleted,
        Reactions = entry.Reactions,
        IsMine = entry.IsMine,
        TimeDisplay = entry.TimeDisplay
    };
}