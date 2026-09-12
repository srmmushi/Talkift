using System.Text.Json.Serialization;

namespace Talkift.Client.Models.V2;

public sealed class AttachmentModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("file_name")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("file_size")]
    public long FileSize { get; set; }

    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("thumbnail_url")]
    public string? ThumbnailUrl { get; set; }

    public string DisplaySize
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

    public bool IsImage => MimeType?.StartsWith("image/") == true ||
                           FileName.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase) ||
                           FileName.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase) ||
                           FileName.EndsWith(".jpeg", System.StringComparison.OrdinalIgnoreCase) ||
                           FileName.EndsWith(".gif", System.StringComparison.OrdinalIgnoreCase) ||
                           FileName.EndsWith(".bmp", System.StringComparison.OrdinalIgnoreCase);
}
