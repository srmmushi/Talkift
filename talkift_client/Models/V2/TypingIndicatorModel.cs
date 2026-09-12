using System.Text.Json.Serialization;

namespace Talkift.Client.Models.V2;

public sealed class TypingIndicatorModel
{
    [JsonPropertyName("conversation_id")]
    public string ConversationId { get; set; } = string.Empty;

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("is_typing")]
    public bool IsTyping { get; set; }

    public string DisplayText => IsTyping ? $"{Username} is typing..." : string.Empty;
}
