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
            try
            {
                var mainWindow = App.CurrentWindow as MainWindow;
                if (mainWindow == null) return;
                _mainViewModel = mainWindow.ViewModel;

                await ViewModel.LoadServersAsync();
                UpdateEmptyState();
                ApplyLocalization();
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("ServerListView_Loaded", ex);
            }
        }

        private void ApplyLocalization()
        {
            try
            {
                HeaderText.Text = LanguageService.GetString("Servers");
                EmptyStateText.Text = LanguageService.GetString("NoServersAdded");
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("ServerListView.ApplyLocalization", ex);
            }
        }

        public void UpdateEmptyState()
        {
            try
            {
                if (EmptyStateText != null)
                {
                    EmptyStateText.Visibility = ViewModel.Servers.Count == 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("ServerListView.UpdateEmptyState", ex);
            }
        }

        private async void ServerListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is Server server)
                {
                    await ViewModel.HandleServerClickAsync(server, _mainViewModel);

                    if (_mainViewModel.IsLoggedIn)
                    {
                        _mainViewModel.CurrentPage = PageType.Chat;
                        var mainWindow = App.CurrentWindow as MainWindow;
                        mainWindow?.UpdateUserInfo();
                        mainWindow?.NavigateToChat();
                    }
                    else
                    {
                        await ShowLoginDialogAsync(server);
                    }
                }
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("ServerListView_ItemClick", ex);
            }
        }

        private void ServerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private async Task ShowLoginDialogAsync(Server server)
        {
            try
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

                    var mainWindow = App.CurrentWindow as MainWindow;
                    mainWindow?.UpdateUserInfo();
                    mainWindow?.NavigateToChat();
                }
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("ShowLoginDialogAsync", ex);
            }
        }

        private async Task ShowRegisterDialogAsync(Server server)
        {
            try
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

                    var mainWindow = App.CurrentWindow as MainWindow;
                    mainWindow?.UpdateUserInfo();
                    mainWindow?.NavigateToChat();
                }
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("ShowRegisterDialogAsync", ex);
            }
        }

        private async void EditServer_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                CrashLogger.LogException("EditServer_Click", ex);
            }
        }

        private async void DeleteServer_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                CrashLogger.LogException("DeleteServer_Click", ex);
            }
        }

        private static readonly Brush OnlineBrush = new SolidColorBrush(Colors.Green);
        private static readonly Brush OfflineBrush = new SolidColorBrush(Colors.Gray);

        public static string Localized(string key)
        {
            return LanguageService.GetString(key);
        }

        public static Windows.UI.Color GetStatusColor(bool isOnline)
        {
            return isOnline ? Colors.Green : Colors.Gray;
        }

        public static Brush GetStatusBrush(bool isOnline)
        {
            return isOnline ? OnlineBrush : OfflineBrush;
        }

        public static string GetStatusText(bool isOnline)
        {
            return isOnline ? LanguageService.GetString("Online") : LanguageService.GetString("Offline");
        }
    }
}
