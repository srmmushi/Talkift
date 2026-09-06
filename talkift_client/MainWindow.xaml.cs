using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Talkift.Client.Models;
using Talkift.Client.ViewModels;
using Talkift.Client.Views;

namespace Talkift.Client
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; } = new();

        public Conversation? CurrentConversation { get; set; }
        public ChatViewModel? ChatViewModel { get; set; }
        public ConversationListViewModel? ConversationListViewModel { get; set; }

        public MainWindow()
        {
            this.InitializeComponent();
            NavView.Loaded += NavView_Loaded;
            ContentFrame.Navigated += ContentFrame_Navigated;

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBar);

            ContentFrame.Navigate(typeof(ServerListView));
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(ServerListView));
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else if (args.InvokedItemContainer?.Tag?.ToString() == "Servers")
            {
                ContentFrame.Navigate(typeof(ServerListView));
            }
            else if (args.InvokedItemContainer?.Tag?.ToString() == "Conversations")
            {
                ContentFrame.Navigate(typeof(ConversationListPage));
            }
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.SourcePageType == typeof(SettingsPage))
            {
                NavView.SelectedItem = NavView.SettingsItem;
            }
            else if (e.SourcePageType == typeof(ServerListView))
            {
                foreach (var item in NavView.MenuItems)
                {
                    if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == "Servers")
                    {
                        NavView.SelectedItem = item;
                        break;
                    }
                }
            }
            else if (e.SourcePageType == typeof(ConversationListPage))
            {
                foreach (var item in NavView.MenuItems)
                {
                    if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == "Conversations")
                    {
                        NavView.SelectedItem = item;
                        break;
                    }
                }
            }

            NavView.IsBackEnabled = ContentFrame.CanGoBack;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(SettingsPage));
        }

        public void NavigateToChat(Conversation? conversation = null)
        {
            if (conversation != null)
                CurrentConversation = conversation;
            ContentFrame.Navigate(typeof(ChatPage));
        }

        public void NavigateToConversationList()
        {
            ContentFrame.Navigate(typeof(ConversationListPage));
        }

        public void NavigateToConversationSettings()
        {
            ContentFrame.Navigate(typeof(ConversationSettingsPage));
        }

        public void NavigateToServerList()
        {
            ContentFrame.Navigate(typeof(ServerListView));
        }

        public void UpdateUserInfo()
        {
            if (ViewModel.IsLoggedIn && !string.IsNullOrEmpty(ViewModel.CurrentUsername))
            {
                UserInfoPanel.Visibility = Visibility.Visible;
                UsernameText.Text = ViewModel.CurrentUsername;

                var glyph = MainViewModel.GetRegisterMethodGlyph(ViewModel.CurrentRegisterMethod);
                var color = MainViewModel.GetRegisterMethodColor(ViewModel.CurrentRegisterMethod);
                UserMethodIcon.Glyph = glyph;
                UserMethodIcon.Foreground = new SolidColorBrush(color);
            }
            else
            {
                UserInfoPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Logout();
            UpdateUserInfo();
            NavigateToServerList();
        }
    }
}
