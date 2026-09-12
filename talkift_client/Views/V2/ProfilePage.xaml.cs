using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.ViewModels.V2;

namespace Talkift.Client.Views.V2;

public sealed partial class ProfilePage : Page
{
    public ProfileViewModel ViewModel { get; } = new(null!, null!, null!, null!);

    public ProfilePage()
    {
        this.InitializeComponent();
        this.Loaded += ProfilePage_Loaded;
    }

    private void ProfilePage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel = new ProfileViewModel(
                App.AuthService!,
                App.ThemeService!,
                App.ChatEngine!,
                App.LoggerService!
            );
            DataContext = ViewModel;
            _ = ViewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            CrashLogger.LogException("ProfilePage_Loaded", ex);
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        _ = ViewModel.UpdateProfileAsync();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CancelEdit();
    }

    private void StatusCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (StatusCombo.SelectedItem is ComboBoxItem item && item.Content is string status)
        {
            _ = ViewModel.ChangeStatusAsync(status);
        }
    }
}
