using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Talkift.Client.Services;
using Talkift.Client.ViewModels.V2;

namespace Talkift.Client.Views.V2;

public sealed partial class ChatPage : Page
{
    public ChatPageViewModel ViewModel { get; set; } = new(null!, null!, null!, null!, null!);

    public ChatPage()
    {
        this.InitializeComponent();
        this.Loaded += ChatPage_Loaded;
        this.Unloaded += ChatPage_Unloaded;
    }

    private void ChatPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel = new ChatPageViewModel(
                App.ChatEngine!,
                App.UiEngine!,
                App.MessageStore!,
                App.NotificationService!,
                App.LoggerService!
            );

            DataContext = ViewModel;

            var mainWindow = App.CurrentWindow as ShellWindow;
            var conv = mainWindow?.ViewModel.SelectedConversation;
            if (conv != null)
            {
                _ = ViewModel.InitializeAsync(
                    new Models.Server { Address = "47.113.216.177", Port = 8002 },
                    conv, "current_user");
            }

            ReplyBar.CancelReply += ReplyBar_CancelReply;
        }
        catch (Exception ex)
        {
            CrashLogger.LogException("ChatPage_Loaded", ex);
        }
    }

    private void ChatPage_Unloaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel.Dispose();
        }
        catch { }
    }

    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        _ = ViewModel.SendMessageAsync();
    }

    private void MessageInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && !e.KeyStatus.IsMenuKeyDown)
        {
            e.Handled = true;
            _ = ViewModel.SendMessageAsync();
        }
    }

    private void MessageInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        // MessageText is two-way bound, so this is handled by the binding
    }

    private void ReplyBar_CancelReply(object sender, EventArgs e)
    {
        ViewModel.CancelReply();
    }
}