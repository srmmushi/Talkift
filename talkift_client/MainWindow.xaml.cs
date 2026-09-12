using System;
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
            try
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
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        ApplyLocalization();
                        RefreshCurrentPage();
                    });
                };

                ContentFrame.Navigate(typeof(ServerListView));
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("MainWindow constructor", ex);
                throw;
            }
        }

        private async System.Threading.Tasks.Task InitBackdropAsync()
        {
            try
            {
                await BackdropService.LoadBackdropAsync();
                await System.Threading.Tasks.Task.Delay(200);
                BackdropService.ApplyCurrentBackdrop(this);
                BackdropService.ApplyOpacity(RootGrid, BackdropService.CurrentOpacity);
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("InitBackdropAsync", ex);
            }
        }

        private void ApplyLocalization()
        {
            try
            {
                ServersNavItem.Content = LanguageService.GetString("Servers");
                AddServerNavText.Text = LanguageService.GetString("AddServer");
                LogoutButtonText.Text = LanguageService.GetString("Logout");
                AppTitleText.Text = LanguageService.GetString("AppTitle");
                Title = LanguageService.GetString("AppTitle");
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("ApplyLocalization", ex);
            }
        }

        private void RefreshCurrentPage()
        {
            try
            {
                var currentPage = ContentFrame.CurrentSourcePageType;
                if (currentPage != null)
                {
                    ContentFrame.Navigate(currentPage);
                }
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("RefreshCurrentPage", ex);
            }
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentFrame.Navigate(typeof(ServerListView));
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("NavView_Loaded", ex);
            }
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            try
            {
                if (args.IsSettingsInvoked)
                {
                    ContentFrame.Navigate(typeof(SettingsPage));
                }
                else if (args.InvokedItemContainer?.Tag?.ToString() == "Servers")
                {
                    ContentFrame.Navigate(typeof(ServerListView));
                }
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("NavView_ItemInvoked", ex);
            }
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            try
            {
                if (ContentFrame.CanGoBack)
                {
                    ContentFrame.GoBack();
                }
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("NavView_BackRequested", ex);
            }
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            try
            {
                if (e.SourcePageType == typeof(SettingsPage))
                {
                    NavView.SelectedItem = NavView.SettingsItem;
                }
                else if (e.SourcePageType == typeof(ServerListView))
                {
                    NavView.SelectedItem = ServersNavItem;
                }

                NavView.IsBackEnabled = ContentFrame.CanGoBack;
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("ContentFrame_Navigated", ex);
            }
        }

        private async void AddServerNavButton_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                CrashLogger.LogException("AddServerNavButton_Click", ex);
            }
        }

        public void NavigateToServerList()
        {
            try { ContentFrame.Navigate(typeof(ServerListView)); }
            catch (Exception ex) { CrashLogger.LogException("NavigateToServerList", ex); }
        }

        public void NavigateToConversationList(Server? server = null)
        {
            try
            {
                if (server != null && ViewModel.SelectedServer == null)
                {
                    ViewModel.SelectServer(server);
                }
                ContentFrame.Navigate(typeof(ConversationListPage));
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("NavigateToConversationList", ex);
            }
        }

        public void NavigateToChat(Conversation? conversation = null)
        {
            try
            {
                if (conversation != null)
                    CurrentConversation = conversation;
                ContentFrame.Navigate(typeof(ChatPage));
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("NavigateToChat", ex);
            }
        }

        public void NavigateToConversationSettings()
        {
            try { ContentFrame.Navigate(typeof(ConversationSettingsPage)); }
            catch (Exception ex) { CrashLogger.LogException("NavigateToConversationSettings", ex); }
        }

        public void UpdateUserInfo()
        {
            try
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
            catch (Exception ex)
            {
                CrashLogger.LogException("UpdateUserInfo", ex);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.Logout();
                UpdateUserInfo();
                NavigateToServerList();
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("LogoutButton_Click", ex);
            }
        }
    }
}
