using System;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Models;
using Talkift.Client.Services;

namespace Talkift.Client.Views
{
    public sealed partial class AddServerDialog : UserControl
    {
        public string ServerName => ServerNameBox.Text?.Trim() ?? string.Empty;
        public string ServerAddress => AddressBox.Text?.Trim() ?? string.Empty;
        public int ServerPort => (int)PortBox.Value;
        public string ServerPassword => PasswordBox.Password ?? string.Empty;

        private bool _isEditing;

        public AddServerDialog()
        {
            this.InitializeComponent();
        }

        public AddServerDialog(Server existing) : this()
        {
            _isEditing = true;
            ServerNameBox.Text = existing.Name;
            AddressBox.Text = existing.Address;
            PortBox.Value = existing.Port;
            PasswordBox.Password = existing.Password;

            if (IsIpAddress(existing.Address))
            {
                PortPanel.Visibility = Visibility.Visible;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try { ApplyLocalization(); }
            catch (Exception ex) { CrashLogger.LogException("AddServerDialog.OnLoaded", ex); }
        }

        private void ApplyLocalization()
        {
            ((TextBlock)ServerNameBox.Header).Text = LanguageService.GetString("ServerNameRequired");
            ServerNameBox.PlaceholderText = LanguageService.GetString("ServerName");
            ((TextBlock)AddressBox.Header).Text = LanguageService.GetString("ServerAddressRequired");
            AddressBox.PlaceholderText = "example.com or 192.168.1.1";
            ((TextBlock)PortBox.Header).Text = LanguageService.GetString("Port");
            ((TextBlock)PasswordBox.Header).Text = LanguageService.GetString("PasswordOptional");
            PasswordBox.PlaceholderText = LanguageService.GetString("Password");
        }

        private void AddressBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var address = AddressBox.Text?.Trim() ?? string.Empty;
                PortPanel.Visibility = IsIpAddress(address) ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("AddressBox_TextChanged", ex);
            }
        }

        private static bool IsIpAddress(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            if (IPAddress.TryParse(input, out _)) return true;
            return Regex.IsMatch(input, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$");
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(ServerName))
            {
                ShowValidation(LanguageService.GetString("UsernameRequired"));
                return false;
            }

            if (string.IsNullOrWhiteSpace(ServerAddress))
            {
                ShowValidation(LanguageService.GetString("ServerAddressRequired"));
                return false;
            }

            ValidationText.Visibility = Visibility.Collapsed;
            return true;
        }

        private void ShowValidation(string message)
        {
            ValidationText.Text = message;
            ValidationText.Visibility = Visibility.Visible;
        }
    }
}
