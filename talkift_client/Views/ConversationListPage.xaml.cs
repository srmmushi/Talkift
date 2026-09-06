using System;
using System.Threading.Tasks;
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
            try
            {
                ApplyLocalization();

                var mainWindow = App.CurrentWindow as MainWindow;
                if (mainWindow == null) return;

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
            catch (Exception ex)
            {
                CrashLogger.LogException("ConversationListPage_Loaded", ex);
            }
        }

        private void ApplyLocalization()
        {
            try
            {
                HeaderText.Text = LanguageService.GetString("Conversations");
                GroupButtonText.Text = LanguageService.GetString("Group");
                ChatButtonText.Text = LanguageService.GetString("Chat");
                EmptyText.Text = LanguageService.GetString("NoConversations");
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("ConversationListPage.ApplyLocalization", ex);
            }
        }

        private async void ConversationListPage_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.ConversationSelected -= OnConversationSelected;
                ViewModel.ErrorOccurred -= OnError;
                ViewModel.JoinRequestReceived -= OnJoinRequest;
                await ViewModel.DisconnectAsync();
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("ConversationListPage_Unloaded", ex);
            }
        }

        private void OnConversationSelected(Conversation conversation)
        {
            try
            {
                var mainWindow = App.CurrentWindow as MainWindow;
                if (mainWindow == null) return;
                mainWindow.CurrentConversation = conversation;
                mainWindow.NavigateToChat(conversation);
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("OnConversationSelected", ex);
            }
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
            try
            {
                var dialog = new ContentDialog
                {
                    Title = LanguageService.GetString("JoinRequest"),
                    Content = $"{fromUser} wants to join {groupName}",
                    PrimaryButtonText = LanguageService.GetString("Accept"),
                    SecondaryButtonText = LanguageService.GetString("Reject"),
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
            catch (Exception ex)
            {
                CrashLogger.LogException("HandleJoinRequestAsync", ex);
            }
        }

        private async void ConversationListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is Conversation conversation)
                {
                    ViewModel.SelectConversation(conversation);
                }
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("ConversationListView_ItemClick", ex);
            }
        }

        private async void CreateGroupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new CreateGroupDialog
                {
                    XamlRoot = this.XamlRoot,
                    WebSocketService = ViewModel.WsService
                };
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("CreateGroupButton_Click", ex);
            }
        }

        private async void CreateConvButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new CreateConversationDialog
                {
                    XamlRoot = this.XamlRoot,
                    WebSocketService = ViewModel.WsService
                };
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("CreateConvButton_Click", ex);
            }
        }
    }
}
