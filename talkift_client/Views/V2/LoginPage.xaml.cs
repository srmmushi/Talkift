using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.ViewModels.V2;
using Talkift.Client.Services;

namespace Talkift.Client.Views.V2;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel { get; } = new(null!, null!, null!, null!);

    public LoginPage()
    {
        this.InitializeComponent();
        this.Loaded += LoginPage_Loaded;
    }

    private void LoginPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel = new LoginViewModel(
                App.AuthService!,
                App.ChatEngine!,
                App.ThemeService!,
                App.LoggerService!
            );
            DataContext = ViewModel;
            _ = ViewModel.InitializeAsync();
            ApplyLocalization();
        }
        catch (Exception ex)
        {
            CrashLogger.LogException("LoginPage_Loaded", ex);
        }
    }

    private void ApplyLocalization()
    {
        try
        {
            UsernameBox.PlaceholderText = "Username";
            PasswordBox.PlaceholderText = "Password";
        }
        catch { }
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await ViewModel.LoginAsync();
            if (ViewModel.IsLoggedIn)
            {
                App.NavigateToConversationList();
            }
        }
        catch (Exception ex)
        {
            CrashLogger.LogException("LoginButton_Click", ex);
        }
    }

    private async void RegisterHyperlink_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await ViewModel.RegisterAsync();
            if (ViewModel.IsLoggedIn)
            {
                App.NavigateToConversationList();
            }
        }
        catch (Exception ex)
        {
            CrashLogger.LogException("RegisterHyperlink_Click", ex);
        }
    }

    private async void OfflineLoginButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await ViewModel.LoadOfflineAsync();
            if (ViewModel.IsLoggedIn)
            {
                App.NavigateToConversationList();
            }
        }
        catch (Exception ex)
        {
            CrashLogger.LogException("OfflineLoginButton_Click", ex);
        }
    }
}
