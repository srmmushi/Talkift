using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Talkift.Client.Models
{
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string RegisterMethod { get; set; } = "local";
        public DateTime CreatedAt { get; set; }
        public DateTime LastLoginAt { get; set; }
    }

    public class Server
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Port { get; set; } = 8080;
        public string Password { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class LoginServer
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Port { get; set; } = 8081;
        public LoginServerType Type { get; set; } = LoginServerType.Official;
    }

    public enum LoginServerType
    {
        Official,
        Local,
        ThirdParty
    }

    public enum PageType
    {
        ServerList,
        ConversationList,
        Chat,
        Settings
    }

    public enum BackdropType
    {
        Default,
        Mica,
        Acrylic
    }

    public class Conversation
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

        public string LastMessage { get; set; } = string.Empty;
        public string LastMessageTime { get; set; } = string.Empty;
        public int UnreadCount { get; set; }
    }

    public class ChatMessage
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

        public bool IsMine { get; set; }
        public string TimeDisplay { get; set; } = string.Empty;
    }

    public class WSMessage
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("payload")]
        public object? Payload { get; set; }
    }

    public class ChatPayload
    {
        [JsonPropertyName("conversation_id")]
        public string ConversationId { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    public class CreateGroupPayload
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("members")]
        public List<string> Members { get; set; } = new();
    }

    public class CreateConversationPayload
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("target_username")]
        public string TargetUsername { get; set; } = string.Empty;
    }

    public class LoadHistoryPayload
    {
        [JsonPropertyName("conversation_id")]
        public string ConversationId { get; set; } = string.Empty;

        [JsonPropertyName("before")]
        public string Before { get; set; } = string.Empty;
    }

    public class DndPayload
    {
        [JsonPropertyName("conversation_id")]
        public string ConversationId { get; set; } = string.Empty;

        [JsonPropertyName("muted")]
        public bool Muted { get; set; }
    }

    public class MutePayload
    {
        [JsonPropertyName("group_id")]
        public string GroupId { get; set; } = string.Empty;

        [JsonPropertyName("target_username")]
        public string TargetUsername { get; set; } = string.Empty;
    }

    public class LeaveGroupPayload
    {
        [JsonPropertyName("group_id")]
        public string GroupId { get; set; } = string.Empty;
    }
}
