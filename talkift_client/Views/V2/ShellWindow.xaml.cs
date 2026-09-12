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
    public ShellViewModel ViewModel { get; } = new(null!, null!, null!, null!, null!, null!);
    private bool _disposed;

    public ShellWindow()
    {
        this.InitializeComponent();
        this.Loaded += ShellWindow_Loaded;
    }

    private void ShellWindow_Loaded(object sender, RoutedEventArgs e)
    {
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

            DataContext = ViewModel;
            ApplyLocalization();
        }
        catch (Exception ex)
        {
            CrashLogger.LogException("ShellWindow_Loaded", ex);
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

    protected override void OnClosed()
    {
        if (!_disposed)
        {
            ViewModel.Dispose();
            _disposed = true;
        }
        base.OnClosed();
    }
}
