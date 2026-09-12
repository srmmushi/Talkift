using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Talkift.Client.Engines;

public sealed class ChatEngineOptions
{
    public string ServerAddress { get; set; } = "47.113.216.177";
    public int ServerPort { get; set; } = 8002;
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(2);
    public TimeSpan MaxReconnectDelay { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxReconnectAttempts { get; set; } = 10;
    public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan SendTimeout { get; set; } = TimeSpan.FromSeconds(10);
    public int MaxRetryCount { get; set; } = 3;
    public int MessageQueueCapacity { get; set; } = 200;

    public string WebSocketUrl => $"ws://{ServerAddress}:{ServerPort}";

    public static ChatEngineOptions FromAddress(string address, int port) => new()
    {
        ServerAddress = address,
        ServerPort = port
    };
}
