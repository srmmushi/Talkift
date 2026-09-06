using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Talkift.Client.Models;
using Talkift.Client.Services;
using Talkift.Client.ViewModels;

namespace Talkift.Client.Views
{
    public sealed partial class ServerListView : Page
    {
        private MainViewModel _mainViewModel = null!;
        public ServerListViewModel ViewModel { get; } = new();

        private readonly AuthService _authService = new();
        private readonly CredentialService _credentialService = new();

        public ServerListView()
        {
            this.InitializeComponent();
            this.Loaded += ServerListView_Loaded;
        }

        private async void ServerListView_Loaded(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Window.Current;
            _mainViewModel = mainWindow.ViewModel;

            await ViewModel.LoadServersAsync();
            UpdateEmptyState();
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            HeaderText.Text = LanguageService.GetString("Servers");
            EmptyStateText.Text = LanguageService.GetString("NoServersAdded");
            EditMenuItem.Text = LanguageService.GetString("EditServer");
            DeleteMenuItem.Text = LanguageService.GetString("Delete");
            StatusText.Text = LanguageService.GetString("Offline");
        }

        public void UpdateEmptyState()
        {
            if (EmptyStateText != null)
            {
                EmptyStateText.Visibility = ViewModel.Servers.Count == 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private async void ServerListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Server server)
            {
                await ViewModel.HandleServerClickAsync(server, _mainViewModel);

                if (_mainViewModel.IsLoggedIn)
                {
                    _mainViewModel.CurrentPage = PageType.Chat;
                    var mainWindow = (MainWindow)Window.Current;
                    mainWindow.UpdateUserInfo();
                    mainWindow.NavigateToChat();
                }
                else
                {
                    await ShowLoginDialogAsync(server);
                }
            }
        }

        private void ServerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private async Task ShowLoginDialogAsync(Server server)
        {
            var loginDialog = new LoginDialog(server, _authService, _credentialService);

            var contentDialog = new ContentDialog
            {
                Title = $"{LanguageService.GetString("LoginToServer")} - {server.Name}",
                PrimaryButtonText = null,
                CloseButtonText = LanguageService.GetString("Cancel"),
                Content = loginDialog,
                XamlRoot = this.XamlRoot
            };

            loginDialog.RegisterRequested += async (s, args) =>
            {
                contentDialog.Hide();
                await ShowRegisterDialogAsync(server);
            };

            var result = await contentDialog.ShowAsync();

            if (loginDialog.LastResult?.Success == true)
            {
                _mainViewModel.SelectServer(server);
                _mainViewModel.OnLoginSuccess(
                    loginDialog.LastResult.Username ?? string.Empty,
                    loginDialog.LastResult.RegisterMethod ?? "local");

                var mainWindow = (MainWindow)Window.Current;
                mainWindow.UpdateUserInfo();
                mainWindow.NavigateToChat();
            }
        }

        private async Task ShowRegisterDialogAsync(Server server)
        {
            List<LoginServer>? loginServers = null;
            try
            {
                var storage = new StorageService();
                loginServers = await storage.LoadAsync<List<LoginServer>>("login_servers");
            }
            catch { }

            var registerDialog = new RegisterDialog(server, _authService, loginServers);

            var contentDialog = new ContentDialog
            {
                Title = $"{LanguageService.GetString("CreateAccount")} - {server.Name}",
                PrimaryButtonText = null,
                CloseButtonText = LanguageService.GetString("Cancel"),
                Content = registerDialog,
                XamlRoot = this.XamlRoot
            };

            registerDialog.LoginRequested += async (s, args) =>
            {
                contentDialog.Hide();
                await ShowLoginDialogAsync(server);
            };

            var result = await contentDialog.ShowAsync();

            if (registerDialog.LastResult?.Success == true)
            {
                _mainViewModel.SelectServer(server);
                _mainViewModel.OnLoginSuccess(
                    registerDialog.LastResult.Username ?? string.Empty,
                    registerDialog.LastResult.RegisterMethod ?? registerDialog.SelectedMode.ToString().ToLower());

                var mainWindow = (MainWindow)Window.Current;
                mainWindow.UpdateUserInfo();
                mainWindow.NavigateToChat();
            }
        }

        private async void EditServer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is Server server)
            {
                var dialog = new ContentDialog
                {
                    Title = LanguageService.GetString("EditServer"),
                    PrimaryButtonText = LanguageService.GetString("Save"),
                    CloseButtonText = LanguageService.GetString("Cancel"),
                    DefaultButton = ContentDialogButton.Primary,
                    Content = new AddServerDialog(server),
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    var editDialog = (AddServerDialog)dialog.Content;
                    server.Name = editDialog.ServerName;
                    server.Address = editDialog.ServerAddress;
                    server.Port = editDialog.ServerPort;
                    server.Password = editDialog.ServerPassword;
                    await ViewModel.EditServerAsync(server);
                }
            }
        }

        private async void DeleteServer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is Server server)
            {
                var confirmText = string.Format(LanguageService.GetString("ConfirmDelete"), server.Name);
                var confirmDialog = new ContentDialog
                {
                    Title = LanguageService.GetString("DeleteServer"),
                    Content = confirmText,
                    PrimaryButtonText = LanguageService.GetString("Delete"),
                    CloseButtonText = LanguageService.GetString("Cancel"),
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
                };

                var result = await confirmDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await ViewModel.RemoveServerAsync(server);
                    UpdateEmptyState();
                }
            }
        }

        public static Windows.UI.Color GetStatusColor(bool isOnline)
        {
            return isOnline ? Colors.Green : Colors.Gray;
        }

        public static string GetStatusText(bool isOnline)
        {
            return isOnline ? "Online" : "Offline";
        }
    }
}
