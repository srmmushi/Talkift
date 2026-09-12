using System.Text.Json.Serialization;

namespace Talkift.Client.Models.V2;

public enum SendResultStatus
{
    Success,
    Failed,
    Timeout,
    NotConnected,
    RateLimited
}

public sealed class SendResult
{
    public SendResultStatus Status { get; init; }
    public string? MessageId { get; init; }
    public string? Error { get; init; }
    public long Timestamp { get; init; }

    public static SendResult Ok(string messageId, long timestamp) => new()
    {
        Status = SendResultStatus.Success,
        MessageId = messageId,
        Timestamp = timestamp
    };

    public static SendResult Fail(string error) => new()
    {
        Status = SendResultStatus.Failed,
        Error = error
    };

    public static SendResult Timeout() => new()
    {
        Status = SendResultStatus.Timeout,
        Error = "Send timed out"
    };

    public static SendResult NotConnected() => new()
    {
        Status = SendResultStatus.NotConnected,
        Error = "Not connected to server"
    };
}
