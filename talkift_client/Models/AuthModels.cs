using System;
using System.Text.Json.Serialization;

namespace Talkift.Client.Models
{
    public class AuthRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Email { get; set; }

        [JsonPropertyName("register_method")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RegisterMethod { get; set; }
    }

    public class AuthResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("uuid")]
        public string? UUID { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("register_method")]
        public string? RegisterMethod { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    public class StoredCredential
    {
        public string ServerId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string RegisterMethod { get; set; } = string.Empty;
        public string ServerAddress { get; set; } = string.Empty;
        public int ServerPort { get; set; }
        public DateTime SavedAt { get; set; } = DateTime.Now;
    }

    public enum RegisterMode
    {
        Official,
        Local
    }
}
