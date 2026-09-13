using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Talkift.Client.Config;

namespace Talkift.Client.Engines;

public sealed class ChatEngineOptions
{
    public string ServerAddress { get; set; } = ServerConfig.DefaultServerAddress;
    public int ServerPort { get; set; } = ServerConfig.DefaultChatPort;
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
