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
        private bool _passwordVisible;

        public AuthResponse? LastResult { get; private set; }
        public bool RememberPassword => RememberPasswordCheckBox.IsChecked == true;

        public LoginDialog(Server server, AuthService authService, CredentialService credentialService)
        {
            this.InitializeComponent();
            _server = server;
            _authService = authService;
            _credentialService = credentialService;
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
                ShowError("Username is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ShowError("Password is required.");
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
                ShowError(LastResult.Message ?? "Login failed.");
            }
        }

        private async void OfflineLoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameBox.Text))
            {
                ShowError("Username is required.");
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
                ShowError(LastResult.Message ?? "Offline login failed.");
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

        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            _passwordVisible = !_passwordVisible;
            if (_passwordVisible)
            {
                PasswordBoxReveal.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordBoxReveal.Password = PasswordBox.Password;
                TogglePasswordIcon.Glyph = "\uE891";
            }
            else
            {
                PasswordBox.Visibility = Visibility.Visible;
                PasswordBoxReveal.Visibility = Visibility.Collapsed;
                PasswordBox.Password = PasswordBoxReveal.Password;
                TogglePasswordIcon.Glyph = "\uE890";
            }
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
