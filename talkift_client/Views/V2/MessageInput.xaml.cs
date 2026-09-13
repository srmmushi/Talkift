using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Talkift.Client.Services;
using Talkift.Client.ViewModels.V2;

namespace Talkift.Client.Views.V2;

public sealed partial class MessageInput : Page
{
    public MessageInputViewModel ViewModel { get; set; } = new(null!, null!);

    public MessageInput()
    {
        this.InitializeComponent();
        this.Loaded += MessageInput_Loaded;
    }

    private void MessageInput_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel = new MessageInputViewModel(App.ChatEngine!, App.LoggerService!);
            DataContext = ViewModel;
        }
        catch (Exception ex)
        {
            CrashLogger.LogException("MessageInput_Loaded", ex);
        }
    }

    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Send();
    }

    private void MessageInputBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && !e.KeyStatus.IsMenuKeyDown)
        {
            e.Handled = true;
            ViewModel.Send();
        }
    }

    private void MessageInputBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.Text = MessageInputBox.Text;
        ViewModel.CanSend = !string.IsNullOrWhiteSpace(MessageInputBox.Text);
    }
}
