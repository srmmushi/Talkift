using System;
using System.Threading;
using System.Threading.Tasks;

namespace Talkift.Client.Engines;

public enum AppTheme
{
    Light,
    Dark,
    System
}

public enum BackdropType
{
    None,
    Mica,
    Acrylic
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

public sealed class NotificationRequestedEventArgs : EventArgs
{
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public NotificationType Type { get; init; }
    public int DurationMs { get; init; } = 3000;
}

public interface IUiEngine : IDisposable
{
    AppTheme CurrentTheme { get; }
    BackdropType CurrentBackdrop { get; }
    double CurrentOpacity { get; }

    event EventHandler<AppTheme>? ThemeChanged;
    event EventHandler<NotificationRequestedEventArgs>? NotificationRequested;

    Task InitializeAsync(UiEngineOptions options, CancellationToken ct = default);
    Task SetThemeAsync(AppTheme theme, CancellationToken ct = default);
    Task SetBackdropAsync(BackdropType backdrop, CancellationToken ct = default);
    Task SetOpacityAsync(double opacity, CancellationToken ct = default);
    Task ShowNotificationAsync(string title, string message, NotificationType type = NotificationType.Info, int durationMs = 3000, CancellationToken ct = default);
    Task NavigateToAsync(string pageKey, object? parameter = null, CancellationToken ct = default);
    Task NavigateBackAsync(CancellationToken ct = default);
    Task RunOnUIAsync(Action action, CancellationToken ct = default);
    Task<T> RunOnUIAsync<T>(Func<T> func, CancellationToken ct = default);
    void RegisterHotkey(string key, Action callback);
    void UnregisterHotkey(string key);
}
