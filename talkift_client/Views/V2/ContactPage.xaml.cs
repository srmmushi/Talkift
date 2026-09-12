using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.ViewModels.V2;

namespace Talkift.Client.Views.V2;

public sealed partial class ContactPage : Page
{
    public ContactViewModel ViewModel { get; } = new(null!, null!, null!);

    public ContactPage()
    {
        this.InitializeComponent();
        this.Loaded += ContactPage_Loaded;
    }

    private void ContactPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel = new ContactViewModel(
                App.ChatEngine!,
                App.AuthService!,
                App.LoggerService!
            );
            DataContext = ViewModel;
            _ = ViewModel.LoadContactsAsync();
        }
        catch (Exception ex)
        {
            CrashLogger.LogException("ContactPage_Loaded", ex);
        }
    }

    private void ChatButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button btn && btn.DataContext is Models.V2.UserProfileModel contact)
            {
                App.NavigateToChat(contact.Username);
            }
        }
        catch (Exception ex)
        {
            CrashLogger.LogException("ChatButton_Click", ex);
        }
    }
}
