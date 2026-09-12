using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.ViewModels.V2;

namespace Talkift.Client.Views.V2;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; } = new(null!, null!, null!, null!, null!, null!);

    public SettingsPage()
    {
        this.InitializeComponent();
        this.Loaded += SettingsPage_Loaded;
    }

    private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel = new SettingsViewModel(
                App.ThemeService!,
                App.AuthService!,
                App.ChatEngine!,
                App.UiEngine!,
                App.NotificationService!,
                App.LoggerService!
            );
            DataContext = ViewModel;
            _ = ViewModel.InitializeAsync();
            ApplyLocalization();
        }
        catch (Exception ex)
        {
            CrashLogger.LogException("SettingsPage_Loaded", ex);
        }
    }

    private void ApplyLocalization()
    {
        try
        {
            NavAppearance.Content = "Appearance";
            NavServerMgmt.Content = "Server";
            NavStorage.Content = "Storage";
            NavAbout.Content = "About";
        }
        catch { }
    }

    private void NavAppearance_Click(object sender, RoutedEventArgs e)
    {
        AppearanceSection.Visibility = Visibility.Visible;
        ServerSection.Visibility = Visibility.Collapsed;
        StorageSection.Visibility = Visibility.Collapsed;
        SaveButton.Visibility = Visibility.Collapsed;
        _ = ViewModel.SaveAsync();
    }

    private void NavServerMgmt_Click(object sender, RoutedEventArgs e)
    {
        AppearanceSection.Visibility = Visibility.Collapsed;
        ServerSection.Visibility = Visibility.Visible;
        StorageSection.Visibility = Visibility.Collapsed;
        SaveButton.Visibility = Visibility.Collapsed;
        _ = ViewModel.SaveOfficialServerAsync();
    }

    private void NavStorage_Click(object sender, RoutedEventArgs e)
    {
        AppearanceSection.Visibility = Visibility.Collapsed;
        ServerSection.Visibility = Visibility.Collapsed;
        StorageSection.Visibility = Visibility.Visible;
        SaveButton.Visibility = Visibility.Collapsed;
    }

    private void NavAbout_Click(object sender, RoutedEventArgs e)
    {
        AppearanceSection.Visibility = Visibility.Collapsed;
        ServerSection.Visibility = Visibility.Collapsed;
        StorageSection.Visibility = Visibility.Collapsed;
        SaveButton.Visibility = Visibility.Visible;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        _ = ViewModel.SaveAsync();
    }

    private async void SaveServerButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveOfficialServerAsync();
    }

    private async void ChangeStoragePath_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ChangeStoragePathAsync();
    }

    private async void OpenLogFolder_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ChangeLogPathAsync();
    }
}
