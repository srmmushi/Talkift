using System;
using System.Collections.Generic;
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
        private readonly List<LoginServer> _loginServers;
        private bool _pwd1Visible;
        private bool _pwd2Visible;

        public AuthResponse? LastResult { get; private set; }
        public RegisterMode SelectedMode { get; private set; } = RegisterMode.Official;

        public RegisterDialog(Server server, AuthService authService, List<LoginServer>? loginServers = null)
        {
            this.InitializeComponent();
            _server = server;
            _authService = authService;
            _loginServers = loginServers ?? new List<LoginServer>();

            foreach (var ls in _loginServers)
            {
                if (ls.Type == LoginServerType.ThirdParty || ls.Type == LoginServerType.Local)
                {
                    ThirdPartyServerCombo.Items.Add($"{ls.Name} ({ls.Address}:{ls.Port})");
                }
            }

            if (ThirdPartyServerCombo.Items.Count > 0)
            {
                ThirdPartyServerCombo.SelectedIndex = 0;
            }
        }

        private void MethodRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MethodRadioButtons.SelectedItem is RadioButton rb)
            {
                SelectedMode = rb.Tag?.ToString() switch
                {
                    "Official" => RegisterMode.Official,
                    "Local" => RegisterMode.Local,
                    "ThirdParty" => RegisterMode.ThirdParty,
                    _ => RegisterMode.Official
                };

                ThirdPartyPanel.Visibility = SelectedMode == RegisterMode.ThirdParty
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorInfoBar.IsOpen = false;

            if (string.IsNullOrWhiteSpace(UsernameBox.Text))
            {
                ShowError("Username is required.");
                return;
            }

            if (UsernameBox.Text.Trim().Length < 3)
            {
                ShowError("Username must be at least 3 characters.");
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ShowError("Password is required.");
                return;
            }

            if (PasswordBox.Password.Length < 6)
            {
                ShowError("Password must be at least 6 characters.");
                return;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                ShowError("Passwords do not match.");
                return;
            }

            string? thirdPartyServer = null;
            if (SelectedMode == RegisterMode.ThirdParty)
            {
                if (ThirdPartyServerCombo.SelectedIndex < 0)
                {
                    ShowError("Please select a registration server.");
                    return;
                }
                var selectedServer = _loginServers[ThirdPartyServerCombo.SelectedIndex];
                thirdPartyServer = $"{selectedServer.Address}:{selectedServer.Port}";
            }

            SetLoading(true);
            LastResult = await _authService.RegisterAsync(
                UsernameBox.Text.Trim(),
                PasswordBox.Password,
                SelectedMode,
                _server.Address,
                _server.Port,
                thirdPartyServer);
            SetLoading(false);

            if (!LastResult.Success)
            {
                ShowError(LastResult.Message ?? "Registration failed.");
            }
        }

        public event EventHandler? LoginRequested;

        private void LoginLink_Click(object sender, RoutedEventArgs e)
        {
            LoginRequested?.Invoke(this, EventArgs.Empty);
        }

        private void TogglePassword1_Click(object sender, RoutedEventArgs e)
        {
            _pwd1Visible = !_pwd1Visible;
            ToggleVisibility(PasswordBox, ToggleIcon1, _pwd1Visible);
        }

        private void TogglePassword2_Click(object sender, RoutedEventArgs e)
        {
            _pwd2Visible = !_pwd2Visible;
            ToggleVisibility(ConfirmPasswordBox, ToggleIcon2, _pwd2Visible);
        }

        private void ToggleVisibility(PasswordBox pwdBox, FontIcon icon, bool showPlain)
        {
            if (showPlain)
            {
                icon.Glyph = "\uE891";
            }
            else
            {
                icon.Glyph = "\uE890";
            }
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
