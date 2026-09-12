using System;
using System.Threading;
using System.Threading.Tasks;

namespace Talkift.Client.Services.V2;

public sealed class NotificationService : INotificationService
{
    private bool _disposed;

    public event EventHandler<NotificationEventArgs>? NotificationRaised;

    public Task ShowAsync(string title, string message, NotificationType type = NotificationType.Info, int durationMs = 3000, CancellationToken ct = default)
    {
        NotificationRaised?.Invoke(this, new NotificationEventArgs
        {
            Title = title,
            Message = message,
            Type = type,
            DurationMs = durationMs
        });
        return Task.CompletedTask;
    }

    public Task ShowInfoAsync(string message, CancellationToken ct = default)
        => ShowAsync("Info", message, NotificationType.Info, ct: ct);

    public Task ShowSuccessAsync(string message, CancellationToken ct = default)
        => ShowAsync("Success", message, NotificationType.Success, ct: ct);

    public Task ShowWarningAsync(string message, CancellationToken ct = default)
        => ShowAsync("Warning", message, NotificationType.Warning, ct: ct);

    public Task ShowErrorAsync(string message, CancellationToken ct = default)
        => ShowAsync("Error", message, NotificationType.Error, 5000, ct);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
