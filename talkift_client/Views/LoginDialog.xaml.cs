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
        private bool _useEmail;

        public AuthResponse? LastResult { get; private set; }

        public event EventHandler? RegisterRequested;

        public LoginDialog(Server server, AuthService authService, CredentialService credentialService)
        {
            this.InitializeComponent();
            _server = server;
            _authService = authService;
            _credentialService = credentialService;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try { ApplyLocalization(); }
            catch (Exception ex) { CrashLogger.LogException("LoginDialog.OnLoaded", ex); }
        }

        private void ApplyLocalization()
        {
            DialogTitle.Text = LanguageService.GetString("LoginToServer");
            ((TextBlock)UsernameBox.Header).Text = LanguageService.GetString("Username");
            UsernameBox.PlaceholderText = LanguageService.GetString("Username");
            ((TextBlock)EmailBox.Header).Text = "Email";
            EmailBox.PlaceholderText = "Enter your email";
            ((TextBlock)PasswordBox.Header).Text = LanguageService.GetString("Password");
            PasswordBox.PlaceholderText = LanguageService.GetString("Password");
            RememberPasswordCheckBox.Content = LanguageService.GetString("RememberPassword");
            LoginButton.Content = LanguageService.GetString("Login");
            OfflineLoginButton.Content = LanguageService.GetString("OfflineLogin");
            NoAccountText.Text = LanguageService.GetString("DontHaveAccount");
            RegisterLinkText.Text = " " + LanguageService.GetString("Register");
        }

        private void LoginModeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            _useEmail = LoginModeToggle.IsOn;
            UsernameBox.Visibility = _useEmail ? Visibility.Collapsed : Visibility.Visible;
            EmailBox.Visibility = _useEmail ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorInfoBar.IsOpen = false;

            if (_useEmail)
            {
                if (string.IsNullOrWhiteSpace(EmailBox.Text))
                {
                    ShowError("Please enter your email.");
                    return;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(UsernameBox.Text))
                {
                    ShowError(LanguageService.GetString("UsernameRequired"));
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ShowError(LanguageService.GetString("PasswordRequired"));
                return;
            }

            SetLoading(true);

            LastResult = _useEmail
                ? await _authService.LoginWithEmailAsync(EmailBox.Text.Trim(), PasswordBox.Password, _server.Address, _server.Port)
                : await _authService.LoginAsync(UsernameBox.Text.Trim(), PasswordBox.Password, _server.Address, _server.Port);

            SetLoading(false);

            if (LastResult.Success)
            {
                if (RememberPasswordCheckBox.IsChecked == true)
                {
                    await _credentialService.SaveCredentialAsync(new StoredCredential
                    {
                        ServerId = _server.Id,
                        Username = LastResult.Username ?? UsernameBox.Text.Trim(),
                        Token = LastResult.Token ?? "",
                        RegisterMethod = LastResult.RegisterMethod ?? "local",
                        ServerAddress = _server.Address,
                        ServerPort = _server.Port
                    });
                }

                var parentDialog = this.Parent as ContentDialog;
                parentDialog?.Hide();
            }
            else
            {
                ShowError(LastResult.Message ?? LanguageService.GetString("LoginFailed"));
            }
        }

        private async void OfflineLoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorInfoBar.IsOpen = false;

            if (string.IsNullOrWhiteSpace(UsernameBox.Text))
            {
                ShowError(LanguageService.GetString("UsernameRequired"));
                return;
            }

            SetLoading(true);
            LastResult = await _authService.OfflineLoginAsync(UsernameBox.Text.Trim(), _server.Address, _server.Port);
            SetLoading(false);

            if (LastResult.Success)
            {
                var parentDialog = this.Parent as ContentDialog;
                parentDialog?.Hide();
            }
            else
            {
                ShowError(LastResult.Message ?? LanguageService.GetString("OfflineLoginFailed"));
            }
        }

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
