using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Talkift.Client.Models.V2;

public sealed class ServerEnvelope
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public JsonElement? Payload { get; set; }

    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    public T? DeserializePayload<T>()
    {
        if (Payload == null || Payload.Value.ValueKind == JsonValueKind.Undefined)
            return default;

        return JsonSerializer.Deserialize<T>(Payload.Value.GetRawText(), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public static ServerEnvelope Parse(string json)
    {
        return JsonSerializer.Deserialize<ServerEnvelope>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new ServerEnvelope();
    }
}

public sealed class ServerMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public object? Payload { get; set; }

    public string Serialize()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }
}

public sealed class ChatPayload
{
    [JsonPropertyName("conversation_id")]
    public string ConversationId { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

public sealed class TypingPayload
{
    [JsonPropertyName("conversation_id")]
    public string ConversationId { get; set; } = string.Empty;

    [JsonPropertyName("is_typing")]
    public bool IsTyping { get; set; }
}

public sealed class ReactionPayload
{
    [JsonPropertyName("message_id")]
    public string MessageId { get; set; } = string.Empty;

    [JsonPropertyName("conversation_id")]
    public string ConversationId { get; set; } = string.Empty;

    [JsonPropertyName("emoji")]
    public string Emoji { get; set; } = string.Empty;
}

public sealed class LoadHistoryPayload
{
    [JsonPropertyName("conversation_id")]
    public string ConversationId { get; set; } = string.Empty;

    [JsonPropertyName("before")]
    public string? Before { get; set; }
}

public sealed class CreateGroupPayload
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("members")]
    public List<string> Members { get; set; } = new();
}

public sealed class CreateConversationPayload
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("target_username")]
    public string TargetUsername { get; set; } = string.Empty;
}
