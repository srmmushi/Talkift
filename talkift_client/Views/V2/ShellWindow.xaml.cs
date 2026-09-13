using System;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Engines;
using Talkift.Client.ViewModels.V2;
using Talkift.Client.Services;

namespace Talkift.Client.Views.V2;

public sealed partial class ShellWindow : Window
{
    public ShellViewModel ViewModel { get; set; } = null!;
    private bool _disposed;

    public ShellWindow()
    {
        this.InitializeComponent();
        this.Closed += ShellWindow_Closed;

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBar);
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));

        try
        {
            ViewModel = new ShellViewModel(
                App.ChatEngine!,
                App.UiEngine!,
                App.AuthService!,
                App.ThemeService!,
                App.NotificationService!,
                App.LoggerService!
            );
            var uiEngine = (UiEngine)App.UiEngine;
            uiEngine.RegisterPage("Login", typeof(LoginPage));
            uiEngine.RegisterPage("ConversationList", typeof(ConversationList));
            uiEngine.RegisterPage("Chat", typeof(ChatPage));
            uiEngine.RegisterPage("Settings", typeof(SettingsPage));
            uiEngine.RegisterPage("Profile", typeof(ProfilePage));
            uiEngine.RegisterPage("Contact", typeof(ContactPage));
            uiEngine.RegisterPage("Search", typeof(SearchPanel));
            uiEngine.RegisterPage("MessageList", typeof(MessageList));
            uiEngine.RegisterPage("MessageInput", typeof(MessageInput));
            _ = uiEngine.InitializeAsync(new UiEngineOptions());
            ApplyLocalization();
            var creds = App.AuthService.LoadSavedCredentialsAsync(CancellationToken.None).GetAwaiter().GetResult();
            var initialPage = (creds.Success && creds.Username != null) ? "ConversationList" : "Login";
            _ = uiEngine.NavigateToAsync(initialPage);
        }
        catch (Exception ex)
        {
            CrashLogger.LogException("ShellWindow_ctor", ex);
        }
    }

    private void ApplyLocalization()
    {
        try
        {
            Title = ViewModel.AppTitle;
        }
        catch { }
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            App.AuthService?.LogoutAsync().GetAwaiter().GetResult();
            App.NavigateToServerList();
        }
        catch (Exception ex)
        {
            CrashLogger.LogException("LogoutButton_Click", ex);
        }
    }

    private void ShellWindow_Closed(object sender, Microsoft.UI.Xaml.WindowEventArgs args)
    {
        if (!_disposed)
        {
            ViewModel.Dispose();
            _disposed = true;
        }
    }
}
