using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Talkift.Client.Models;
using Talkift.Client.ViewModels;

namespace Talkift.Client.Views
{
    public sealed partial class ChatPage : Page
    {
        public ChatViewModel ViewModel { get; } = new();

        public ChatPage()
        {
            this.InitializeComponent();
            this.Loaded += ChatPage_Loaded;
            this.Unloaded += ChatPage_Unloaded;
        }

        private async void ChatPage_Loaded(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Window.Current;
            if (mainWindow.ViewModel.SelectedServer is Server server
                && mainWindow.ViewModel.CurrentUser is User user)
            {
                ViewModel.NewMessageReceived += OnNewMessage;
                ViewModel.ErrorOccurred += OnError;

                if (mainWindow.CurrentConversation != null)
                {
                    await ViewModel.InitializeAsync(server, mainWindow.CurrentConversation, user.Id);
                }
            }
        }

        private async void ChatPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.NewMessageReceived -= OnNewMessage;
            ViewModel.ErrorOccurred -= OnError;
            await ViewModel.DisconnectAsync();
        }

        private void OnNewMessage(ChatMessage message)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                MessagesList.ScrollIntoView(MessagesList.Items[^1]);
            });
        }

        private void OnError(string error)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                ErrorInfoBar.Message = error;
                ErrorInfoBar.IsOpen = true;
            });
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MessageText = MessageInput.Text;
            await ViewModel.SendMessageAsync();
            MessageInput.Text = string.Empty;
            SendButton.IsEnabled = false;
            MessageInput.Focus(FocusState.Programmatic);
        }

        private async void MessageInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                ViewModel.MessageText = MessageInput.Text;
                await ViewModel.SendMessageAsync();
                MessageInput.Text = string.Empty;
                SendButton.IsEnabled = false;
            }
        }

        private void MessageInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            SendButton.IsEnabled = !string.IsNullOrWhiteSpace(MessageInput.Text);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Window.Current;
            mainWindow.NavigateToConversationSettings();
        }
    }
}
