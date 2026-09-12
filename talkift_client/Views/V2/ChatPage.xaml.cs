using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Talkift.Client.ViewModels.V2;
using Talkift.Client.Views.Controls;

namespace Talkift.Client.Views.V2;

public sealed partial class ChatPage : Page
{
    public ChatPageViewModel ViewModel { get; } = new(null!, null!, null!, null!, null!);

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
                    new Models.V2.Server { Address = "47.113.216.177", Port = 8002 },
                    conv, "current_user");
            }

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
        else if (e.Key == Windows.System.VirtualKey.Enter && e.KeyStatus.IsMenuKeyDown)
        {
            // Shift+Enter = newline (handled by TextBox)
        }
    }

    private void MessageInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        // MessageText is two-way bound, so this is handled by the binding
    }

    private void SearchPanel_SearchRequested(object sender, string query)
    {
        _ = ViewModel.SearchMessagesAsync(query);
    }

    private void SearchPanel_CloseRequested(object sender, EventArgs e)
    {
        ViewModel.IsSearchPanelVisible = false;
    }

    private void EmojiPicker_EmojiSelected(object sender, string emoji)
    {
        ViewModel.MessageText += emoji;
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
                FilePreview.SetFile(file.Path);
        }
        catch (Exception ex) { CrashLogger.LogException("FilePicker", ex); }
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
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.CurrentWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var dialog = new Controls.ImagePreviewDialog();
                dialog.SetImage(file.Path);
                dialog.XamlRoot = this.XamlRoot;
                await dialog.ShowAsync();
            }
        }
        catch (Exception ex) { CrashLogger.LogException("ImagePicker", ex); }
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
        ViewModel.CancelReply();
    }

    private void ReactionBar_ReactionAdded(object sender, (string MessageId, string Emoji) e)
    {
        _ = ViewModel.SendReactionAsync(e.Emoji);
    }

    private void FilePreview_RemoveRequested(object sender, EventArgs e)
    {
        FilePreview.Hide();
    }
}
