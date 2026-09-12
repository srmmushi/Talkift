using System;
using System.Threading;
using System.Threading.Tasks;

namespace Talkift.Client.Services;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

public sealed class LogEntry
{
    public DateTime Timestamp { get; init; }
    public LogLevel Level { get; init; }
    public string Source { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Exception? Exception { get; init; }
}

public interface ILoggerService : IDisposable
{
    void Debug(string source, string message);
    void Info(string source, string message);
    void Warning(string source, string message);
    void Error(string source, string message, Exception? ex = null);
    void Fatal(string source, string message, Exception? ex = null);
    Task FlushAsync(CancellationToken ct = default);
}
