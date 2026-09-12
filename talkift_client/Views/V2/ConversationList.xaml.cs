using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.ViewModels.V2;

namespace Talkift.Client.Views.V2;

public sealed partial class ConversationList : Page
{
    public ConversationListViewModel ViewModel { get; } = new(null!, null!, null!, null!);

    public ConversationList()
    {
        this.InitializeComponent();
        this.Loaded += ConversationList_Loaded;
    }

    private void ConversationList_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel = new ConversationListViewModel(
                App.ChatEngine!,
                App.AuthService!,
                App.MessageStore!,
                App.LoggerService!
            );
            DataContext = ViewModel;
            _ = ViewModel.InitializeAsync(
                new Models.V2.Server { Address = "47.113.216.177", Port = 8002 },
                "current_user", "User");
        }
        catch (Exception ex)
        {
            CrashLogger.LogException("ConversationList_Loaded", ex);
        }
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            App.NavigateToChat();
        }
        catch (Exception ex)
        {
            CrashLogger.LogException("AddButton_Click", ex);
        }
    }
}
