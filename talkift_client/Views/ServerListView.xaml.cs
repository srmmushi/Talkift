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
        }

        private void UpdateEmptyState()
        {
            if (EmptyStateText != null)
            {
                EmptyStateText.Visibility = ViewModel.Servers.Count == 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private async void AddServerButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Add Server",
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                Content = new AddServerDialog(),
                XamlRoot = this.XamlRoot
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

                await ViewModel.AddServerAsync(server);
                UpdateEmptyState();
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
                Title = $"Login to {server.Name}",
                PrimaryButtonText = null,
                CloseButtonText = "Cancel",
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
                Title = $"Register on {server.Name}",
                PrimaryButtonText = null,
                CloseButtonText = "Cancel",
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
                    Title = "Edit Server",
                    PrimaryButtonText = "Save",
                    CloseButtonText = "Cancel",
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
                var confirmDialog = new ContentDialog
                {
                    Title = "Delete Server",
                    Content = $"Are you sure you want to delete \"{server.Name}\"?",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
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
