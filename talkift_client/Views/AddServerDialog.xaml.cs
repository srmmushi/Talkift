using System;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Models;

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

        private void AddressBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var address = AddressBox.Text?.Trim() ?? string.Empty;
            if (IsIpAddress(address))
            {
                PortPanel.Visibility = Visibility.Visible;
            }
            else
            {
                PortPanel.Visibility = Visibility.Collapsed;
            }
        }

        private static bool IsIpAddress(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (IPAddress.TryParse(input, out _))
                return true;

            return Regex.IsMatch(input, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$");
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(ServerName))
            {
                ShowValidation("Server name is required.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(ServerAddress))
            {
                ShowValidation("Server address is required.");
                return false;
            }

            if (IsIpAddress(ServerAddress))
            {
                if (!IPAddress.TryParse(ServerAddress, out _))
                {
                    ShowValidation("Invalid IP address format.");
                    return false;
                }

                if (ServerPort < 1 || ServerPort > 65535)
                {
                    ShowValidation("Port must be between 1 and 65535.");
                    return false;
                }
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
