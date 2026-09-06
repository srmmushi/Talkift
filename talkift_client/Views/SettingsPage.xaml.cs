using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Models;
using Talkift.Client.ViewModels;

namespace Talkift.Client.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; } = new();

        public SettingsPage()
        {
            this.InitializeComponent();
            this.Loaded += SettingsPage_Loaded;
        }

        private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadSettingsAsync();
        }

        private async void AddLoginServer_Click(object sender, RoutedEventArgs e)
        {
            var nameBox = new TextBox { Header = "Name", PlaceholderText = "My Login Server" };
            var addressBox = new TextBox { Header = "Address", PlaceholderText = "localhost" };
            var portBox = new NumberBox { Header = "Port", Value = 8081, Minimum = 1, Maximum = 65535 };
            var typeCombo = new ComboBox { Header = "Type" };
            typeCombo.Items.Add("Official");
            typeCombo.Items.Add("Local");
            typeCombo.Items.Add("ThirdParty");
            typeCombo.SelectedIndex = 0;

            var panel = new StackPanel { Spacing = 12 };
            panel.Children.Add(nameBox);
            panel.Children.Add(addressBox);
            panel.Children.Add(portBox);
            panel.Children.Add(typeCombo);

            var dialog = new ContentDialog
            {
                Title = "Add Login Server",
                Content = panel,
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var serverType = typeCombo.SelectedItem?.ToString() switch
                {
                    "Local" => LoginServerType.Local,
                    "ThirdParty" => LoginServerType.ThirdParty,
                    _ => LoginServerType.Official
                };

                var server = new LoginServer
                {
                    Name = nameBox.Text?.Trim() ?? string.Empty,
                    Address = addressBox.Text?.Trim() ?? string.Empty,
                    Port = (int)portBox.Value,
                    Type = serverType
                };

                await ViewModel.AddLoginServerAsync(server);
            }
        }

        private async void DeleteLoginServer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is LoginServer server)
            {
                var confirm = new ContentDialog
                {
                    Title = "Delete Login Server",
                    Content = $"Remove \"{server.Name}\"?",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
                };

                var result = await confirm.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await ViewModel.RemoveLoginServerAsync(server);
                }
            }
        }
    }
}
