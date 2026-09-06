using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Models;
using Talkift.Client.Services;

namespace Talkift.Client.Views
{
    public sealed partial class ConversationSettingsPage : Page
    {
        public string ConversationName { get; set; } = string.Empty;
        public string ConversationType { get; set; } = string.Empty;
        public string ConversationCreated { get; set; } = string.Empty;
        public ObservableCollection<string> Members { get; } = new();
        public bool IsOwnerVisible { get; set; } = false;
        public bool IsDndVisible { get; set; } = true;

        private Conversation? _conversation;
        private string _currentUserId = string.Empty;
        private string _currentUsername = string.Empty;
        private bool _isOwner = false;

        public ConversationSettingsPage()
        {
            this.InitializeComponent();
            this.Loaded += ConversationSettingsPage_Loaded;
        }

        private async void ConversationSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyLocalization();

            var mainWindow = (MainWindow)Window.Current;
            if (mainWindow.CurrentConversation == null || mainWindow.ViewModel.CurrentUser == null)
                return;

            _conversation = mainWindow.CurrentConversation;
            _currentUserId = mainWindow.ViewModel.CurrentUser.Id;
            _currentUsername = mainWindow.ViewModel.CurrentUsername;
            _isOwner = _conversation.OwnerId == _currentUserId;

            ConversationName = _conversation.Name;
            ConversationType = _conversation.IsGroup
                ? LanguageService.GetString("Group")
                : LanguageService.GetString("DirectMessage");
            ConversationCreated = _conversation.CreatedAt;
            IsOwnerVisible = _isOwner;
            IsDndVisible = !_conversation.IsPublic;

            Members.Clear();
            foreach (var m in _conversation.Members)
                Members.Add(m);

            DataContext = this;
            Bindings.Update();

            var wsService = mainWindow.ChatViewModel?.WsService ?? mainWindow.ConversationListViewModel?.WsService;
            if (wsService != null)
            {
                await wsService.SendLoadMembersAsync(_conversation.Id);

                foreach (var m in _conversation.Members)
                {
                    var btn = GetActionButtonForUser(m);
                    if (btn != null)
                    {
                        if (_isOwner && m != _currentUsername)
                        {
                            btn.Content = LanguageService.GetString("Mute");
                            btn.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            btn.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void ApplyLocalization()
        {
            BackToChatText.Text = LanguageService.GetString("BackToChat");
            InfoHeader.Text = LanguageService.GetString("Info");
            NameLabel.Text = LanguageService.GetString("Name");
            TypeLabel.Text = LanguageService.GetString("Type");
            CreatedLabel.Text = LanguageService.GetString("Created");
            MembersHeader.Text = LanguageService.GetString("Members");
            NotificationsHeader.Text = LanguageService.GetString("Notifications");
            ((TextBlock)DndToggle.Header).Text = LanguageService.GetString("DoNotDisturb");
            AdminHeader.Text = LanguageService.GetString("AdminActions");
            DeleteGroupButton.Content = LanguageService.GetString("DeleteGroup");
            LeaveButton.Content = LanguageService.GetString("Leave");
        }

        private Button? GetActionButtonForUser(string username)
        {
            foreach (var item in MembersList.Items)
            {
                if (item is ContentPresenter presenter && presenter.Content is string name && name == username)
                {
                    var btn = FindButtonInContainer(presenter);
                    if (btn != null) return btn;
                }
            }
            return null;
        }

        private Button? FindButtonInContainer(DependencyObject parent)
        {
            for (int i = 0; i < Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is Button btn)
                    return btn;
                var result = FindButtonInContainer(child);
                if (result != null) return result;
            }
            return null;
        }

        private async void MemberActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string username)
            {
                var mainWindow = (MainWindow)Window.Current;
                var wsService = mainWindow.ChatViewModel?.WsService ?? mainWindow.ConversationListViewModel?.WsService;
                if (wsService == null || _conversation == null) return;

                if (btn.Content?.ToString() == LanguageService.GetString("Mute"))
                {
                    await wsService.SendMuteAsync(_conversation.Id, username);
                    btn.Content = LanguageService.GetString("Unmute");
                }
                else
                {
                    await wsService.SendMuteAsync(_conversation.Id, username);
                    btn.Content = LanguageService.GetString("Mute");
                }
            }
        }

        private async void DeleteGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = LanguageService.GetString("DeleteGroup"),
                Content = LanguageService.GetString("DeleteGroupConfirm"),
                PrimaryButtonText = LanguageService.GetString("Delete"),
                SecondaryButtonText = LanguageService.GetString("Cancel"),
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var mainWindow = (MainWindow)Window.Current;
                var wsService = mainWindow.ChatViewModel?.WsService ?? mainWindow.ConversationListViewModel?.WsService;
                if (wsService != null && _conversation != null)
                {
                    await wsService.SendLeaveGroupAsync(_conversation.Id);
                    mainWindow.NavigateToConversationList();
                }
            }
        }

        private async void LeaveButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Window.Current;
            var wsService = mainWindow.ChatViewModel?.WsService ?? mainWindow.ConversationListViewModel?.WsService;
            if (wsService != null && _conversation != null)
            {
                if (_conversation.IsGroup)
                    await wsService.SendLeaveGroupAsync(_conversation.Id);

                mainWindow.NavigateToConversationList();
            }
        }

        private async void DndToggle_Toggled(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Window.Current;
            var wsService = mainWindow.ChatViewModel?.WsService ?? mainWindow.ConversationListViewModel?.WsService;
            if (wsService != null && _conversation != null)
            {
                await wsService.SendDndAsync(_conversation.Id, DndToggle.IsOn);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Window.Current;
            mainWindow.NavigateToChat(mainWindow.CurrentConversation);
        }
    }
}
