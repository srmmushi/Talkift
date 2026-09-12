using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Models;
using Talkift.Client.Services;

namespace Talkift.Client.Views
{
    public sealed partial class RegisterDialog : UserControl
    {
        private readonly AuthService _authService;
        private readonly Server _server;
        private bool _pwd1Visible;
        private bool _pwd2Visible;

        public AuthResponse? LastResult { get; private set; }
        public RegisterMode SelectedMode { get; private set; } = RegisterMode.Official;

        public event EventHandler? LoginRequested;

        public RegisterDialog(Server server, AuthService authService)
        {
            this.InitializeComponent();
            _server = server;
            _authService = authService;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try { ApplyLocalization(); }
            catch (Exception ex) { CrashLogger.LogException("RegisterDialog.OnLoaded", ex); }
        }

        private void ApplyLocalization()
        {
            DialogTitle.Text = LanguageService.GetString("CreateAccount");
            MethodLabel.Text = LanguageService.GetString("RegisterMethod");
            OfficialRadio.Content = LanguageService.GetString("Official");
            LocalRadio.Content = LanguageService.GetString("Local");
            ((TextBlock)UsernameBox.Header).Text = LanguageService.GetString("Username");
            UsernameBox.PlaceholderText = LanguageService.GetString("UsernameMinLength");
            ((TextBlock)EmailBox.Header).Text = LanguageService.GetString("EmailOptional");
            EmailBox.PlaceholderText = LanguageService.GetString("EmailPlaceholder");
            ((TextBlock)PasswordBox.Header).Text = LanguageService.GetString("Password");
            PasswordBox.PlaceholderText = LanguageService.GetString("PasswordMinLength");
            ((TextBlock)ConfirmPasswordBox.Header).Text = LanguageService.GetString("ConfirmPassword");
            ConfirmPasswordBox.PlaceholderText = LanguageService.GetString("ConfirmPasswordPlaceholder");
            RegisterButton.Content = LanguageService.GetString("Register");
            HaveAccountText.Text = LanguageService.GetString("AlreadyHaveAccount");
            LoginLinkText.Text = " " + LanguageService.GetString("Login");
        }

        private void MethodRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (MethodRadioButtons.SelectedItem is RadioButton rb)
                {
                    SelectedMode = rb.Tag?.ToString() switch
                    {
                        "Official" => RegisterMode.Official,
                        "Local" => RegisterMode.Local,
                        _ => RegisterMode.Official
                    };

                    EmailBox.Visibility = SelectedMode == RegisterMode.Official
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("MethodRadioButtons_SelectionChanged", ex);
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorInfoBar.IsOpen = false;

                if (string.IsNullOrWhiteSpace(UsernameBox.Text))
                {
                    ShowError(LanguageService.GetString("UsernameRequired"));
                    return;
                }

                if (UsernameBox.Text.Trim().Length < 3)
                {
                    ShowError(LanguageService.GetString("UsernameMinLength"));
                    return;
                }

                if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    ShowError(LanguageService.GetString("PasswordRequired"));
                    return;
                }

                if (PasswordBox.Password.Length < 6)
                {
                    ShowError(LanguageService.GetString("PasswordMinLength"));
                    return;
                }

                if (PasswordBox.Password != ConfirmPasswordBox.Password)
                {
                    ShowError(LanguageService.GetString("PasswordsNoMatch"));
                    return;
                }

                SetLoading(true);
                LastResult = await _authService.RegisterAsync(
                    UsernameBox.Text.Trim(),
                    PasswordBox.Password,
                    SelectedMode,
                    _server.Address,
                    _server.Port,
                    EmailBox.Visibility == Visibility.Visible ? EmailBox.Text?.Trim() : null);
                SetLoading(false);

                if (!LastResult.Success)
                {
                    ShowError(LastResult.Message ?? LanguageService.GetString("RegistrationFailed"));
                }
                else
                {
                    var parentDialog = this.Parent as ContentDialog;
                    parentDialog?.Hide();
                }
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("RegisterButton_Click", ex);
                SetLoading(false);
                ShowError(LanguageService.GetString("RegistrationFailed"));
            }
        }

        private void LoginLink_Click(object sender, RoutedEventArgs e)
        {
            LoginRequested?.Invoke(this, EventArgs.Empty);
        }

        private void TogglePassword1_Click(object sender, RoutedEventArgs e)
        {
            _pwd1Visible = !_pwd1Visible;
            ToggleIcon1.Glyph = _pwd1Visible ? "\uE891" : "\uE890";
        }

        private void TogglePassword2_Click(object sender, RoutedEventArgs e)
        {
            _pwd2Visible = !_pwd2Visible;
            ToggleIcon2.Glyph = _pwd2Visible ? "\uE891" : "\uE890";
        }

        private void ShowError(string message)
        {
            ErrorInfoBar.Message = message;
            ErrorInfoBar.IsOpen = true;
        }

        private void SetLoading(bool loading)
        {
            LoadingRing.IsActive = loading;
            RegisterButton.IsEnabled = !loading;
        }
    }
}
