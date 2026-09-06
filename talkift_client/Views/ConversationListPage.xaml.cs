using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Models;
using Talkift.Client.Services;
using Talkift.Client.ViewModels;

namespace Talkift.Client.Views
{
    public sealed partial class ConversationListPage : Page
    {
        public ConversationListViewModel ViewModel { get; } = new();

        public ConversationListPage()
        {
            this.InitializeComponent();
            this.Loaded += ConversationListPage_Loaded;
            this.Unloaded += ConversationListPage_Unloaded;
        }

        private async void ConversationListPage_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyLocalization();

            var mainWindow = (MainWindow)Window.Current;
            if (mainWindow.ViewModel.SelectedServer is Server server
                && mainWindow.ViewModel.CurrentUser is User user)
            {
                ViewModel.ConversationSelected += OnConversationSelected;
                ViewModel.ErrorOccurred += OnError;
                ViewModel.JoinRequestReceived += OnJoinRequest;

                await ViewModel.InitializeAsync(server, user.Id, user.Username);
                await ViewModel.LoadConversationsAsync();
            }
        }

        private void ApplyLocalization()
        {
            HeaderText.Text = LanguageService.GetString("Conversations");
            GroupButtonText.Text = LanguageService.GetString("Group");
            ChatButtonText.Text = LanguageService.GetString("Chat");
            EmptyText.Text = LanguageService.GetString("NoConversations");
        }

        private async void ConversationListPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.ConversationSelected -= OnConversationSelected;
            ViewModel.ErrorOccurred -= OnError;
            ViewModel.JoinRequestReceived -= OnJoinRequest;
            await ViewModel.DisconnectAsync();
        }

        private void OnConversationSelected(Conversation conversation)
        {
            var mainWindow = (MainWindow)Window.Current;
            mainWindow.CurrentConversation = conversation;
            mainWindow.NavigateToChat(conversation);
        }

        private void OnError(string error)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                StatusInfoBar.Message = error;
                StatusInfoBar.Severity = InfoBarSeverity.Error;
                StatusInfoBar.IsOpen = true;
            });
        }

        private void OnJoinRequest(string fromUser, string groupName)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                _ = HandleJoinRequestAsync(fromUser, groupName);
            });
        }

        private async Task HandleJoinRequestAsync(string fromUser, string groupName)
        {
            var dialog = new ContentDialog
            {
                Title = "Join Request",
                Content = $"{fromUser} wants to join {groupName}",
                PrimaryButtonText = "Accept",
                SecondaryButtonText = "Reject",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            await ViewModel.WsService.SendAsync(new
            {
                type = "join_response",
                payload = new { group_name = groupName, from_user = fromUser, accepted = result == ContentDialogResult.Primary }
            });
        }

        private async void ConversationListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Conversation conversation)
            {
                ViewModel.SelectConversation(conversation);
            }
        }

        private async void CreateGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Window.Current;
            var dialog = new CreateGroupDialog
            {
                XamlRoot = this.XamlRoot,
                WebSocketService = ViewModel.WsService
            };
            await dialog.ShowAsync();
        }

        private async void CreateConvButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CreateConversationDialog
            {
                XamlRoot = this.XamlRoot,
                WebSocketService = ViewModel.WsService
            };
            await dialog.ShowAsync();
        }
    }
}
