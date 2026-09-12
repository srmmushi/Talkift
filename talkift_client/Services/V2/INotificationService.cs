using System;
using System.Threading;
using System.Threading.Tasks;

namespace Talkift.Client.Services;

public sealed class NotificationEventArgs : EventArgs
{
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public NotificationType Type { get; init; }
    public int DurationMs { get; init; } = 3000;
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

public interface INotificationService : IDisposable
{
    event EventHandler<NotificationEventArgs>? NotificationRaised;

    Task ShowAsync(string title, string message, NotificationType type = NotificationType.Info, int durationMs = 3000, CancellationToken ct = default);
    Task ShowInfoAsync(string message, CancellationToken ct = default);
    Task ShowSuccessAsync(string message, CancellationToken ct = default);
    Task ShowWarningAsync(string message, CancellationToken ct = default);
    Task ShowErrorAsync(string message, CancellationToken ct = default);
}
