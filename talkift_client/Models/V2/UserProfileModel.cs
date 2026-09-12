using System;
using System.Text.Json.Serialization;

namespace Talkift.Client.Models.V2;

public sealed class UserProfileModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "online";

    [JsonPropertyName("status_message")]
    public string? StatusMessage { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("last_seen_at")]
    public DateTime? LastSeenAt { get; set; }

    [JsonPropertyName("register_method")]
    public string RegisterMethod { get; set; } = "local";

    [JsonIgnore]
    public string DisplayName => !string.IsNullOrEmpty(Username) ? Username : Email;

    [JsonIgnore]
    public string Initials
    {
        get
        {
            if (string.IsNullOrEmpty(Username)) return "?";
            var parts = Username.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            return Username.Length >= 2 ? Username[..2].ToUpper() : Username.ToUpper();
        }
    }
}
