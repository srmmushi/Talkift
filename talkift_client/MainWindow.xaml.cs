using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Talkift.Client.Models;
using Talkift.Client.Services;
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

            if (AppWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.PreferredMinimumWidth = 480;
                presenter.PreferredMinimumHeight = 360;
            }

            ApplyLocalization();
            _ = InitBackdropAsync();

            LanguageService.LanguageChanged += () =>
            {
                DispatcherQueue.TryEnqueue(() => ApplyLocalization());
            };

            ContentFrame.Navigate(typeof(ServerListView));
        }

        private async System.Threading.Tasks.Task InitBackdropAsync()
        {
            await BackdropService.LoadBackdropAsync();
            await System.Threading.Tasks.Task.Delay(100);
            BackdropService.ApplyCurrentBackdrop(this);
        }

        private void ApplyLocalization()
        {
            ServersNavItem.Content = LanguageService.GetString("Servers");
            ConversationsNavItem.Content = LanguageService.GetString("Conversations");
            AddServerNavText.Text = LanguageService.GetString("AddServer");
            LogoutButtonText.Text = LanguageService.GetString("Logout");
            AppTitleText.Text = LanguageService.GetString("AppTitle");
            Title = LanguageService.GetString("AppTitle");
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
                NavView.SelectedItem = ServersNavItem;
            }
            else if (e.SourcePageType == typeof(ConversationListPage))
            {
                NavView.SelectedItem = ConversationsNavItem;
            }

            NavView.IsBackEnabled = ContentFrame.CanGoBack;
        }

        private async void AddServerNavButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = LanguageService.GetString("AddServer"),
                PrimaryButtonText = LanguageService.GetString("Add"),
                CloseButtonText = LanguageService.GetString("Cancel"),
                DefaultButton = ContentDialogButton.Primary,
                Content = new AddServerDialog(),
                XamlRoot = this.NavView.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var addDialog = (AddServerDialog)dialog.Content;
                var server = new Server
                {
                    Name = addDialog.ServerName,
                    Address = addDialog.ServerAddress,
                    Port = addDialog.ServerPort,
                    Password = addDialog.ServerPassword
                };

                if (ContentFrame.CurrentSourcePageType == typeof(ServerListView))
                {
                    var serverPage = ContentFrame.Content as ServerListView;
                    if (serverPage != null)
                    {
                        await serverPage.ViewModel.AddServerAsync(server);
                        serverPage.UpdateEmptyState();
                    }
                }
                else
                {
                    NavigateToServerList();
                }
            }
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
