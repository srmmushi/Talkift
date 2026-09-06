using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Talkift.Client.Models;
using Talkift.Client.Services;
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
            try
            {
                ApplyLocalization();

                var mainWindow = App.CurrentWindow as MainWindow;
                if (mainWindow == null) return;

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
            catch (Exception ex)
            {
                CrashLogger.LogException("ChatPage_Loaded", ex);
            }
        }

        private void ApplyLocalization()
        {
            try
            {
                MessageInput.PlaceholderText = LanguageService.GetString("TypeMessage");
                EmptyChatText.Text = LanguageService.GetString("NoMessages");
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("ChatPage.ApplyLocalization", ex);
            }
        }

        private async void ChatPage_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.NewMessageReceived -= OnNewMessage;
                ViewModel.ErrorOccurred -= OnError;
                await ViewModel.DisconnectAsync();
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("ChatPage_Unloaded", ex);
            }
        }

        private void OnNewMessage(ChatMessage message)
        {
            try
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    if (MessagesList.Items.Count > 0)
                    {
                        MessagesList.ScrollIntoView(MessagesList.Items[^1]);
                    }
                });
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("OnNewMessage", ex);
            }
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
            try
            {
                ViewModel.MessageText = MessageInput.Text;
                await ViewModel.SendMessageAsync();
                MessageInput.Text = string.Empty;
                SendButton.IsEnabled = false;
                MessageInput.Focus(FocusState.Programmatic);
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("SendButton_Click", ex);
            }
        }

        private async void MessageInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                CrashLogger.LogException("MessageInput_KeyDown", ex);
            }
        }

        private void MessageInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            SendButton.IsEnabled = !string.IsNullOrWhiteSpace(MessageInput.Text);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mainWindow = App.CurrentWindow as MainWindow;
                mainWindow?.NavigateToConversationSettings();
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("SettingsButton_Click", ex);
            }
        }
    }
}
