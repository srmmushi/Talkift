using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Talkift.Client.Models.V2;

public enum MessageType
{
    Chat,
    Image,
    File,
    Voice,
    System,
    JoinRequest,
    Leave,
    Pin
}

public enum MessageStatus
{
    Sending,
    Sent,
    Delivered,
    Read,
    Failed
}

public sealed class ChatMessageModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("sender_id")]
    public string SenderId { get; set; } = string.Empty;

    [JsonPropertyName("sender_name")]
    public string SenderName { get; set; } = string.Empty;

    [JsonPropertyName("conversation_id")]
    public string ConversationId { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "chat";

    [JsonPropertyName("is_pinned")]
    public bool IsPinned { get; set; }

    [JsonPropertyName("reply_to_message_id")]
    public string? ReplyToMessageId { get; set; }

    [JsonPropertyName("is_edited")]
    public bool IsEdited { get; set; }

    [JsonPropertyName("is_deleted")]
    public bool IsDeleted { get; set; }

    [JsonPropertyName("reactions")]
    public List<string> Reactions { get; set; } = new();

    [JsonPropertyName("attachments")]
    public List<AttachmentModel> Attachments { get; set; } = new();

    [JsonIgnore]
    public bool IsMine { get; set; }

    [JsonIgnore]
    public string TimeDisplay { get; set; } = string.Empty;

    [JsonIgnore]
    public MessageStatus Status { get; set; } = MessageStatus.Sent;

    [JsonIgnore]
    public MessageType ParsedType => Type?.ToLower() switch
    {
        "image" => MessageType.Image,
        "file" => MessageType.File,
        "voice" => MessageType.Voice,
        "system" => MessageType.System,
        "join_request" => MessageType.JoinRequest,
        "leave" => MessageType.Leave,
        "pin" => MessageType.Pin,
        _ => MessageType.Chat
    };

    public string GetFormattedTime()
    {
        var dt = DateTimeOffset.FromUnixTimeSeconds(Timestamp).LocalDateTime;
        var now = DateTime.Now;

        if (dt.Date == now.Date)
            return dt.ToString("HH:mm");
        if (dt.Date == now.Date.AddDays(-1))
            return "Yesterday " + dt.ToString("HH:mm");
        if (dt.Year == now.Year)
            return dt.ToString("MM/dd HH:mm");
        return dt.ToString("yyyy/MM/dd HH:mm");
    }
}
