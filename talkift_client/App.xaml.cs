using System;
using Microsoft.UI.Dispatching;
using Talkift.Client.Engines;
using Talkift.Client.Services;
using Talkift.Client.Services.V2;
using Talkift.Client.ViewModels.V2;

namespace Talkift.Client;

public sealed partial class App : Application
{
    public static IChatEngine ChatEngine { get; private set; } = null!;
    public static IUiEngine UiEngine { get; private set; } = null!;
    public static IAuthService AuthService { get; private set; } = null!;
    public static IThemeService ThemeService { get; private set; } = null!;
    public static INotificationService NotificationService { get; private set; } = null!;
    public static ILoggerService LoggerService { get; private set; } = null!;
    public static IMessageStore MessageStore { get; private set; } = null!;

    public static Window CurrentWindow { get; private set; } = null!;

    private ServiceProvider? _serviceProvider;
    private bool _disposed;

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var host = AppHost.Create();
        _serviceProvider = host.Services;

        ChatEngine = _serviceProvider.GetRequiredService<IChatEngine>();
        UiEngine = _serviceProvider.GetRequiredService<IUiEngine>();
        AuthService = _serviceProvider.GetRequiredService<IAuthService>();
        ThemeService = _serviceProvider.GetRequiredService<IThemeService>();
        NotificationService = _serviceProvider.GetRequiredService<INotificationService>();
        LoggerService = _serviceProvider.GetRequiredService<ILoggerService>();
        MessageStore = _serviceProvider.GetRequiredService<IMessageStore>();

        CurrentWindow = new ShellWindow();
        CurrentWindow.Activate();
    }

    public static T GetRequiredService<T>() where T : class
    {
        return (T)CurrentWindow.Resources["ServiceContainer"] ??
               throw new InvalidOperationException($"Service {typeof(T).Name} not found");
    }

    public void NavigateToServerList()
    {
        CurrentWindow.DispatcherQueue.TryEnqueue(() =>
        {
            // Navigation handled by MainViewModel
        });
    }

    public void NavigateToConversationList()
    {
        CurrentWindow.DispatcherQueue.TryEnqueue(() =>
        {
            // Navigation handled by MainViewModel
        });
    }

    public void NavigateToChat(ConversationModel? conversation = null)
    {
        CurrentWindow.DispatcherQueue.TryEnqueue(() =>
        {
            // Navigation handled by MainViewModel
        });
    }

    protected override void OnSuspending(object sender, SuspendingEventArgs args)
    {
        if (_disposed) return;
        ChatEngine.DisconnectAsync().GetAwaiter().GetResult();
        LoggerService.FlushAsync().GetAwaiter().GetResult();
        _disposed = true;
        base.OnSuspending(sender, args);
    }

    protected override void OnExit()
    {
        ChatEngine?.Dispose();
        LoggerService?.FlushAsync().GetAwaiter().GetResult();
        base.OnExit();
    }
}
