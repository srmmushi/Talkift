using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Talkift.Client.Engines;
using Talkift.Client.Models.V2;
using Talkift.Client.Services;
using Talkift.Client.Services.V2;
using Talkift.Client.ViewModels.V2;
using Talkift.Client.Views.V2;

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

    public static Window CurrentWindow { get; internal set; } = null!;

    private ServiceProvider? _serviceProvider;

    public App()
    {
        UnhandledException += (_, e) =>
        {
            CrashLogger.LogException("App.UnhandledException", e.Exception);
            e.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                CrashLogger.LogException("AppDomain.UnhandledException", ex);
        };
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            CrashLogger.LogMessage("OnLaunched: creating AppHost...");
            var host = AppHost.Create();
            _serviceProvider = host;
            CrashLogger.LogMessage("OnLaunched: AppHost created");

            ChatEngine = _serviceProvider.GetRequiredService<IChatEngine>();
            UiEngine = _serviceProvider.GetRequiredService<IUiEngine>();
            AuthService = _serviceProvider.GetRequiredService<IAuthService>();
            ThemeService = _serviceProvider.GetRequiredService<IThemeService>();
            NotificationService = _serviceProvider.GetRequiredService<INotificationService>();
            LoggerService = _serviceProvider.GetRequiredService<ILoggerService>();
            MessageStore = _serviceProvider.GetRequiredService<IMessageStore>();
            CrashLogger.LogMessage("OnLaunched: services resolved");

            CurrentWindow = new ShellWindow();
            CrashLogger.LogMessage("OnLaunched: ShellWindow created");
            CurrentWindow.Activate();
            CrashLogger.LogMessage("OnLaunched: ShellWindow activated");
        }
        catch (Exception ex)
        {
            CrashLogger.LogException("OnLaunched", ex);
            throw;
        }
    }

    public static void NavigateToServerList()
    {
        (CurrentWindow as ShellWindow)?.ViewModel.NavigateTo("Login");
    }

    public static void NavigateToConversationList()
    {
        (CurrentWindow as ShellWindow)?.ViewModel.NavigateTo("ConversationList");
    }

    public static void NavigateToChat(string? username = null)
    {
        (CurrentWindow as ShellWindow)?.ViewModel.NavigateTo("Chat");
    }
}
