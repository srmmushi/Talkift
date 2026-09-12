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

            SearchPanel.SearchRequested += SearchPanel_SearchRequested;
            SearchPanel.CloseRequested += SearchPanel_CloseRequested;
            EmojiPicker.EmojiSelected += EmojiPicker_EmojiSelected;
            QuickToolbar.EmojiClicked += QuickToolbar_EmojiClicked;
            QuickToolbar.FileClicked += QuickToolbar_FileClicked;
            QuickToolbar.ImageClicked += QuickToolbar_ImageClicked;
            QuickToolbar.VoiceClicked += QuickToolbar_VoiceClicked;
            VoiceRecorder.VoiceReady += VoiceRecorder_VoiceReady;
            ReplyBar.CancelReply += ReplyBar_CancelReply;
            ReactionBar.ReactionAdded += ReactionBar_ReactionAdded;
            FilePreview.RemoveRequested += FilePreview_RemoveRequested;
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
                    ViewModel.UserJoined += OnUserJoined;

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
                ViewModel.UserJoined -= OnUserJoined;
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

        private void OnUserJoined(string username)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                TypingIndicator.Show(username);
                var timer = Microsoft.UI.Xaml.DispatcherQueue.GetForCurrentThread().CreateTimer();
                timer.Duration = TimeSpan.FromSeconds(3);
                timer.Tick += (s, e) => { TypingIndicator.Hide(); timer.Stop(); };
                timer.Start();
            });
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var replyId = ReplyBar.ReplyToMessageId;
                var content = MessageInput.Text;

                if (!string.IsNullOrWhiteSpace(replyId))
                {
                    content = $"[Reply to {ReplyBar.ReplyToSender}]: {content}";
                    ReplyBar.Hide();
                }

                ViewModel.MessageText = content;
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
                    var replyId = ReplyBar.ReplyToMessageId;
                    var content = MessageInput.Text;

                    if (!string.IsNullOrWhiteSpace(replyId))
                    {
                        content = $"[Reply to {ReplyBar.ReplyToSender}]: {content}";
                        ReplyBar.Hide();
                    }

                    ViewModel.MessageText = content;
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

        private void ChatHeaderCtrl_ChatClicked(object sender, EventArgs e)
        {
            var mainWindow = App.CurrentWindow as MainWindow;
            mainWindow?.NavigateToConversationSettings();
        }

        private void ChatHeaderCtrl_SearchClicked(object sender, EventArgs e)
        {
            SearchPanel.Show();
        }

        private void ChatHeaderCtrl_PinClicked(object sender, EventArgs e)
        {
            var pinned = ViewModel.GetPinnedMessages();
            if (pinned.Count > 0)
                PinPanel.Show(pinned);
        }

        private void ChatHeaderCtrl_MembersClicked(object sender, EventArgs e)
        {
        }

        private void ChatHeaderCtrl_MoreClicked(object sender, EventArgs e)
        {
        }

        private void SearchPanel_SearchRequested(object sender, string query)
        {
            var results = ViewModel.SearchMessages(query);
            SearchPanel.SetResultCount(results.Count);
        }

        private void SearchPanel_CloseRequested(object sender, EventArgs e)
        {
            SearchPanel.Hide();
        }

        private void EmojiPicker_EmojiSelected(object sender, string emoji)
        {
            MessageInput.Text += emoji;
            EmojiFlyout.Hide();
        }

        private void QuickToolbar_EmojiClicked(object sender, EventArgs e)
        {
            EmojiFlyout.ShowAt(QuickToolbar);
        }

        private async void QuickToolbar_FileClicked(object sender, EventArgs e)
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.FileTypeFilter.Add("*");
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.CurrentWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    FilePreview.SetFile(file.Path);
                }
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("QuickToolbar_FileClicked", ex);
            }
        }

        private async void QuickToolbar_ImageClicked(object sender, EventArgs e)
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".gif");
                picker.FileTypeFilter.Add(".bmp");
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.CurrentWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    var dialog = new ImagePreviewDialog();
                    dialog.SetImage(file.Path);
                    dialog.XamlRoot = this.XamlRoot;
                    var result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        ViewModel.MessageText = $"[Image: {file.Name}]";
                        await ViewModel.SendMessageAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("QuickToolbar_ImageClicked", ex);
            }
        }

        private void QuickToolbar_VoiceClicked(object sender, EventArgs e)
        {
            VoiceRecorder.StartRecording();
        }

        private void VoiceRecorder_VoiceReady(object sender, string filePath)
        {
            ViewModel.MessageText = $"[Voice: {filePath}]";
            _ = ViewModel.SendMessageAsync();
        }

        private void ReplyBar_CancelReply(object sender, EventArgs e)
        {
            ReplyBar.Hide();
        }

        private void ReactionBar_ReactionAdded(object sender, (string MessageId, string Emoji) e)
        {
            _ = ViewModel.SendMessageAsync();
        }

        private void FilePreview_RemoveRequested(object sender, EventArgs e)
        {
            FilePreview.Hide();
        }

        private void MsgCtx_Copy(object sender, RoutedEventArgs e)
        {
        }

        private void MsgCtx_Reply(object sender, RoutedEventArgs e)
        {
        }

        private void MsgCtx_Pin(object sender, RoutedEventArgs e)
        {
        }

        private void MsgCtx_Delete(object sender, RoutedEventArgs e)
        {
        }
    }
}
