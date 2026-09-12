using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Talkift.Client.Models.V2;

public sealed class ConversationModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("is_group")]
    public bool IsGroup { get; set; }

    [JsonPropertyName("members")]
    public List<string> Members { get; set; } = new();

    [JsonPropertyName("owner_id")]
    public string OwnerId { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("is_public")]
    public bool IsPublic { get; set; }

    [JsonPropertyName("banned_members")]
    public List<string> BannedMembers { get; set; } = new();

    [JsonPropertyName("is_muted")]
    public bool IsMuted { get; set; }

    [JsonIgnore]
    public string LastMessage { get; set; } = string.Empty;

    [JsonIgnore]
    public string LastMessageTime { get; set; } = string.Empty;

    [JsonIgnore]
    public long LastMessageTimestamp { get; set; }

    [JsonIgnore]
    public int UnreadCount { get; set; }

    [JsonIgnore]
    public bool HasUnread => UnreadCount > 0;

    [JsonIgnore]
    public string DisplaySubtitle
    {
        get
        {
            if (!string.IsNullOrEmpty(LastMessage))
                return LastMessage;
            if (IsGroup)
                return $"{Members.Count} members";
            return "No messages yet";
        }
    }

    public string GetFormattedTime()
    {
        if (LastMessageTimestamp <= 0) return string.Empty;
        var dt = DateTimeOffset.FromUnixTimeSeconds(LastMessageTimestamp).LocalDateTime;
        var now = DateTime.Now;

        if (dt.Date == now.Date)
            return dt.ToString("HH:mm");
        if (dt.Date == now.Date.AddDays(-1))
            return "Yesterday";
        if (dt.Year == now.Year)
            return dt.ToString("MM/dd");
        return dt.ToString("yyyy/MM/dd");
    }
}
