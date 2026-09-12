using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Talkift.Client.Services.V2;

public sealed class LoggerService : ILoggerService
{
    private readonly ConcurrentQueue<LogEntry> _queue = new();
    private readonly string _logDir;
    private bool _disposed;
    private bool _flushing;
    private readonly Timer _flushTimer;
    private const int MaxQueueSize = 500;

    public LoggerService()
    {
        _logDir = Path.Combine(StorageService.DataDir, "logs");
        Directory.CreateDirectory(_logDir);
        _flushTimer = new Timer(async _ => await FlushAsync(), null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    public void Debug(string source, string message) => Enqueue(LogLevel.Debug, source, message);
    public void Info(string source, string message) => Enqueue(LogLevel.Info, source, message);
    public void Warning(string source, string message) => Enqueue(LogLevel.Warning, source, message);
    public void Error(string source, string message, Exception? ex = null) => Enqueue(LogLevel.Error, source, message, ex);
    public void Fatal(string source, string message, Exception? ex = null) => Enqueue(LogLevel.Fatal, source, message, ex);

    private void Enqueue(LogLevel level, string source, string message, Exception? ex = null)
    {
        if (_disposed) return;

        _queue.Enqueue(new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Source = source,
            Message = message,
            Exception = ex
        });

        if (_queue.Count > MaxQueueSize)
        {
            _ = FlushAsync();
        }
    }

    public async Task FlushAsync(CancellationToken ct = default)
    {
        if (_disposed || _flushing || _queue.IsEmpty) return;
        _flushing = true;

        try
        {
            var fileName = $"talkift_{DateTime.Now:yyyyMMdd}.log";
            var filePath = Path.Combine(_logDir, fileName);
            var sb = new StringBuilder();

            while (_queue.TryDequeue(out var entry))
            {
                sb.Append(entry.Timestamp.ToString("HH:mm:ss.fff"));
                sb.Append(" [");
                sb.Append(entry.Level);
                sb.Append("] [");
                sb.Append(entry.Source);
                sb.Append("] ");
                sb.Append(entry.Message);
                if (entry.Exception != null)
                {
                    sb.Append(" | ");
                    sb.Append(entry.Exception.GetType().Name);
                    sb.Append(": ");
                    sb.Append(entry.Exception.Message);
                    if (entry.Exception.StackTrace != null)
                    {
                        sb.AppendLine();
                        sb.Append(entry.Exception.StackTrace);
                    }
                }
                sb.AppendLine();
            }

            if (sb.Length > 0)
            {
                await File.AppendAllTextAsync(filePath, sb.ToString(), Encoding.UTF8, ct);
            }
        }
        catch { }
        finally
        {
            _flushing = false;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _flushTimer.Dispose();
        _ = FlushAsync();
        GC.SuppressFinalize(this);
    }
}
