using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Models;
using Talkift.Client.Services;

namespace Talkift.Client.Views
{
    public sealed partial class LoginDialog : UserControl
    {
        private readonly AuthService _authService;
        private readonly CredentialService _credentialService;
        private readonly Server _server;

        public AuthResponse? LastResult { get; private set; }
        public bool RememberPassword => RememberPasswordCheckBox.IsChecked == true;

        public LoginDialog(Server server, AuthService authService, CredentialService credentialService)
        {
            this.InitializeComponent();
            _server = server;
            _authService = authService;
            _credentialService = credentialService;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            DialogTitle.Text = LanguageService.GetString("LoginToServer");
            ((TextBlock)UsernameBox.Header).Text = LanguageService.GetString("Username");
            UsernameBox.PlaceholderText = LanguageService.GetString("Username");
            ((TextBlock)PasswordBox.Header).Text = LanguageService.GetString("Password");
            PasswordBox.PlaceholderText = LanguageService.GetString("Password");
            RememberPasswordCheckBox.Content = LanguageService.GetString("RememberPassword");
            OfflineLoginCheckBox.Content = LanguageService.GetString("OfflineLogin");
            LoginButton.Content = LanguageService.GetString("Login");
            OfflineLoginButton.Content = LanguageService.GetString("Login");
            NoAccountText.Text = LanguageService.GetString("DontHaveAccount");
            RegisterLinkText.Text = " " + LanguageService.GetString("Register");
        }

        public void SetUsername(string username)
        {
            UsernameBox.Text = username;
        }

        public void SetSavedCredential(StoredCredential credential)
        {
            UsernameBox.Text = credential.Username;
            RememberPasswordCheckBox.IsChecked = true;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameBox.Text))
            {
                ShowError(LanguageService.GetString("UsernameRequired"));
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ShowError(LanguageService.GetString("PasswordRequired"));
                return;
            }

            SetLoading(true);
            LastResult = await _authService.LoginAsync(
                UsernameBox.Text.Trim(),
                PasswordBox.Password,
                _server.Address,
                _server.Port);

            if (LastResult.Success && RememberPassword)
            {
                await _credentialService.SaveCredentialAsync(new StoredCredential
                {
                    ServerId = _server.Id,
                    Username = LastResult.Username ?? UsernameBox.Text.Trim(),
                    Token = LastResult.Token ?? string.Empty,
                    RegisterMethod = LastResult.RegisterMethod ?? "local",
                    ServerAddress = _server.Address,
                    ServerPort = _server.Port
                });
            }

            SetLoading(false);

            if (!LastResult.Success)
            {
                ShowError(LastResult.Message ?? LanguageService.GetString("LoginFailed"));
            }
        }

        private async void OfflineLoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameBox.Text))
            {
                ShowError(LanguageService.GetString("UsernameRequired"));
                return;
            }

            SetLoading(true);
            LastResult = await _authService.OfflineLoginAsync(
                UsernameBox.Text.Trim(),
                _server.Address,
                _server.Port);

            if (LastResult.Success && RememberPassword)
            {
                await _credentialService.SaveCredentialAsync(new StoredCredential
                {
                    ServerId = _server.Id,
                    Username = LastResult.Username ?? UsernameBox.Text.Trim(),
                    Token = LastResult.Token ?? string.Empty,
                    RegisterMethod = LastResult.RegisterMethod ?? "local",
                    ServerAddress = _server.Address,
                    ServerPort = _server.Port
                });
            }

            SetLoading(false);

            if (!LastResult.Success)
            {
                ShowError(LastResult.Message ?? LanguageService.GetString("OfflineLoginFailed"));
            }
        }

        private void OfflineLoginCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Visibility = Visibility.Collapsed;
            LoginButton.Visibility = Visibility.Collapsed;
            OfflineLoginButton.Visibility = Visibility.Visible;
        }

        private void OfflineLoginCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Visibility = Visibility.Visible;
            LoginButton.Visibility = Visibility.Visible;
            OfflineLoginButton.Visibility = Visibility.Collapsed;
        }

        public event EventHandler? RegisterRequested;

        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            RegisterRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ShowError(string message)
        {
            ErrorInfoBar.Message = message;
            ErrorInfoBar.IsOpen = true;
        }

        private void SetLoading(bool loading)
        {
            LoadingRing.IsActive = loading;
            LoginButton.IsEnabled = !loading;
            OfflineLoginButton.IsEnabled = !loading;
        }
    }
}
