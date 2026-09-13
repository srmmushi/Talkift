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

    public static Window CurrentWindow { get; private set; } = null!;

    private ServiceProvider? _serviceProvider;
    private bool _disposed;

    public App()
    {
        UnhandledException += (_, e) =>
        {
            WriteCrashLog($"[UnhandledException] {e.Message}{Environment.NewLine}{e.Exception}");
        };
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            WriteCrashLog($"[AppDomain] {e.ExceptionObject}");
        };
    }

    private static void WriteCrashLog(string text)
    {
        try
        {
            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "crash.log");
            System.IO.File.AppendAllText(path, $"[{DateTime.Now:O}] {text}{Environment.NewLine}");
        }
        catch
        {
        }
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            var host = AppHost.Create();
            _serviceProvider = host;

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
        catch (Exception ex)
        {
            WriteCrashLog($"[OnLaunched] {ex}");
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
