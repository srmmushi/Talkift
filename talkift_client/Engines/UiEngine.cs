using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.UI.ViewManagement;

namespace Talkift.Client.Engines;

public sealed class UiEngine : IUiEngine
{
    private Window? _window;
    private Frame? _frame;
    private readonly Dictionary<string, Type> _pageRegistry = new();
    private readonly Dictionary<string, Action> _hotkeyCallbacks = new();
    private readonly Dictionary<string, Microsoft.UI.Xaml.Input.KeyboardAccelerator> _hotkeys = new();
    private AppTheme _currentTheme = AppTheme.System;
    private BackdropType _currentBackdrop = BackdropType.Mica;
    private double _currentOpacity = 1.0;
    private bool _initialized;
    private MicaController? _micaController;
    private AcrylicController? _acrylicController;
    private SystemBackdropConfiguration? _backdropConfig;
    private readonly UISettings _uiSettings = new();
    private AppTheme _actualTheme = AppTheme.Light;

    public AppTheme CurrentTheme => _currentTheme;
    public BackdropType CurrentBackdrop => _currentBackdrop;
    public double CurrentOpacity => _currentOpacity;

    public event EventHandler<AppTheme>? ThemeChanged;
    public event EventHandler<NotificationRequestedEventArgs>? NotificationRequested;

    public Task InitializeAsync(UiEngineOptions options, CancellationToken ct = default)
    {
        if (_initialized) return Task.CompletedTask;
        _initialized = true;

        _window = App.CurrentWindow;
        _frame = FindFrame(_window);

        if (_window?.Content is FrameworkElement root)
        {
            root.ActualThemeChanged += OnActualThemeChanged;
            _uiSettings.ColorValuesChanged += OnColorValuesChanged;
        }

        _ = SetThemeAsync(options.UseMica ? AppTheme.System : AppTheme.Light, ct);
        _ = SetBackdropAsync(options.UseMica ? BackdropType.Mica : options.UseAcrylic ? BackdropType.Acrylic : BackdropType.None, ct);
        _ = SetOpacityAsync(options.DefaultOpacity, ct);

        return Task.CompletedTask;
    }

    public async Task SetThemeAsync(AppTheme theme, CancellationToken ct = default)
    {
        _currentTheme = theme;
        await RunOnUIAsync(() =>
        {
            if (_window?.Content is FrameworkElement root)
            {
                root.RequestedTheme = theme switch
                {
                    AppTheme.Light => ElementTheme.Light,
                    AppTheme.Dark => ElementTheme.Dark,
                    _ => ElementTheme.Default
                };
            }
        }, ct);

        ThemeChanged?.Invoke(this, theme);
    }

    public async Task SetBackdropAsync(BackdropType backdrop, CancellationToken ct = default)
    {
        _currentBackdrop = backdrop;
        await RunOnUIAsync(() =>
        {
            if (_window == null) return;

            try
            {
                _micaController?.Dispose();
                _micaController = null;
                _acrylicController?.Dispose();
                _acrylicController = null;

                switch (backdrop)
                {
                    case BackdropType.Mica:
                        _micaController = new MicaController();
                        _backdropConfig = new SystemBackdropConfiguration();
                        _backdropConfig.IsInputActive = true;
                        _backdropConfig.Theme = SystemBackdropTheme.Dark;
                        _micaController.SetSystemBackdropConfiguration(_backdropConfig);
                        _window.SystemBackdrop = new MicaSystemBackdrop();
                        break;

                    case BackdropType.Acrylic:
                        _window.SystemBackdrop = new AcrylicSystemBackdrop
                        {
                            TintColor = Microsoft.UI.ColorHelper.FromArgb(128, 32, 32, 32)
                        };
                        break;

                    default:
                        _window.SystemBackdrop = null;
                        break;
                }
            }
            catch
            {
                _window.SystemBackdrop = null;
            }
        }, ct);
    }

    public async Task SetOpacityAsync(double opacity, CancellationToken ct = default)
    {
        _currentOpacity = Math.Clamp(opacity, 0.1, 1.0);
        await RunOnUIAsync(() =>
        {
            if (_window?.Content is FrameworkElement root && root.Parent is Grid parent)
            {
                parent.Opacity = _currentOpacity;
            }
        }, ct);
    }

    public async Task ShowNotificationAsync(string title, string message, NotificationType type = NotificationType.Info, int durationMs = 3000, CancellationToken ct = default)
    {
        NotificationRequested?.Invoke(this, new NotificationRequestedEventArgs
        {
            Title = title,
            Message = message,
            Type = type,
            DurationMs = durationMs
        });
        await Task.CompletedTask;
    }

    public async Task NavigateToAsync(string pageKey, object? parameter = null, CancellationToken ct = default)
    {
        await RunOnUIAsync(() =>
        {
            if (_frame == null) return;
            if (!_pageRegistry.TryGetValue(pageKey, out var pageType)) return;

            var transition = new EntranceThemeTransition();
            _frame.Navigate(pageType, parameter, transition);
        }, ct);
    }

    public async Task NavigateBackAsync(CancellationToken ct = default)
    {
        await RunOnUIAsync(() =>
        {
            if (_frame?.CanGoBack == true)
            {
                _frame.GoBack(new DrillInNavigationTransitionInfo());
            }
        }, ct);
    }

    public async Task RunOnUIAsync(Action action, CancellationToken ct = default)
    {
        if (_window?.DispatcherQueue == null)
        {
            action();
            return;
        }

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var reg = ct.Register(() => tcs.TrySetCanceled(ct));

        _window.DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                action();
                tcs.TrySetResult(true);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        await tcs.Task;
        reg.Dispose();
    }

    public async Task<T> RunOnUIAsync<T>(Func<T> func, CancellationToken ct = default)
    {
        if (_window?.DispatcherQueue == null)
            return func();

        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        var reg = ct.Register(() => tcs.TrySetCanceled(ct));

        _window.DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                tcs.TrySetResult(func());
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        var result = await tcs.Task;
        reg.Dispose();
        return result;
    }

    public void RegisterPage(string key, Type pageType)
    {
        _pageRegistry[key] = pageType;
    }

    public void RegisterHotkey(string key, Action callback)
    {
        _hotkeyCallbacks[key] = callback;
    }

    public void UnregisterHotkey(string key)
    {
        _hotkeyCallbacks.Remove(key);
        if (_hotkeys.TryGetValue(key, out var acc))
        {
            _frame?.DispatcherQueue?.TryEnqueue(() =>
            {
                try { _frame?.DispatcherQueue; } catch { }
            });
            _hotkeys.Remove(key);
        }
    }

    private void OnActualThemeChanged(FrameworkElement sender, object args)
    {
        _actualTheme = sender.ActualTheme == ElementTheme.Dark ? AppTheme.Dark : AppTheme.Light;
        if (_currentTheme == AppTheme.System)
        {
            ThemeChanged?.Invoke(this, AppTheme.System);
        }
    }

    private void OnColorValuesChanged(UISettings sender, ColorValuesChangedEventArgs args)
    {
        var isDark = _uiSettings.GetColorValue(UIColorType.Background).R < 128;
        _actualTheme = isDark ? AppTheme.Dark : AppTheme.Light;
    }

    private static Frame? FindFrame(Window? window)
    {
        if (window?.Content is Frame f) return f;
        if (window?.Content is Microsoft.UI.Xaml.Controls.NavigationView nav)
        {
            foreach (var child in nav.Descendants())
            {
                if (child is Frame frame) return frame;
            }
        }
        if (window?.Content is Grid grid)
        {
            foreach (var child in grid.Descendants())
            {
                if (child is Frame frame) return frame;
            }
        }
        return null;
    }

    public void Dispose()
    {
        _micaController?.Dispose();
        _acrylicController?.Dispose();
        GC.SuppressFinalize(this);
    }
}

internal static class FrameworkElementExtensions
{
    public static IEnumerable<Microsoft.UI.Xaml.DependencyObject> Descendants(this Microsoft.UI.Xaml.DependencyObject parent)
    {
        if (parent == null) yield break;
        var count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
            if (child != null)
            {
                yield return child;
                foreach (var descendant in child.Descendants())
                    yield return descendant;
            }
        }
    }
}
