using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Talkift.Client.Models.V2;

public sealed class ServerInfoModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("is_online")]
    public bool IsOnline { get; set; }

    [JsonPropertyName("max_users")]
    public int MaxUsers { get; set; }

    [JsonPropertyName("current_users")]
    public int CurrentUsers { get; set; }
}

public sealed class VersionInfoModel
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("download_url")]
    public string DownloadUrl { get; set; } = string.Empty;

    [JsonPropertyName("release_notes")]
    public string ReleaseNotes { get; set; } = string.Empty;

    [JsonPropertyName("release_date")]
    public DateTime ReleaseDate { get; set; }

    [JsonPropertyName("is_latest")]
    public bool IsLatest { get; set; }

    [JsonPropertyName("file_size")]
    public long FileSize { get; set; }

    public string DisplayFileSize
    {
        get
        {
            if (FileSize > 1024 * 1024)
                return $"{FileSize / (1024.0 * 1024.0):F1} MB";
            if (FileSize > 1024)
                return $"{FileSize / 1024.0:F1} KB";
            return $"{FileSize} B";
        }
    }
}

public sealed class ServerListItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Port { get; set; } = 8080;
    public string Password { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public bool IsOfficial { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public static ServerListItem Official => new()
    {
        Id = "official",
        Name = "Talkift Official",
        Address = "47.113.216.177",
        Port = 8002,
        IsOfficial = true,
        IsOnline = true
    };
}
