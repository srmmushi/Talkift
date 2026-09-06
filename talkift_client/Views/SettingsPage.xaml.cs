using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Models;
using Talkift.Client.Services;
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

            BackdropComboBox.Items.Clear();
            BackdropComboBox.Items.Add(LanguageService.GetString("BackdropDefault"));
            BackdropComboBox.Items.Add(LanguageService.GetString("BackdropMica"));
            BackdropComboBox.Items.Add(LanguageService.GetString("BackdropAcrylic"));
            BackdropComboBox.SelectedIndex = ViewModel.SelectedBackdropIndex;

            LanguageComboBox.Items.Clear();
            LanguageComboBox.Items.Add("English");
            LanguageComboBox.Items.Add("\u4e2d\u6587");
            LanguageComboBox.SelectedIndex = ViewModel.SelectedLanguageIndex;

            ApplyLocalization();
        }

        private async void BackdropComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BackdropComboBox.SelectedIndex >= 0)
            {
                await ViewModel.SaveBackdropAsync(BackdropComboBox.SelectedIndex);
            }
        }

        private async void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedIndex >= 0 && LanguageComboBox.SelectedIndex != ViewModel.SelectedLanguageIndex)
            {
                await ViewModel.SaveLanguageAsync(LanguageComboBox.SelectedIndex);
                ApplyLocalization();

                StatusInfoBar.Message = LanguageService.GetString("LanguageChanged");
                StatusInfoBar.IsOpen = true;
            }
        }

        private async void AutoCheckToggle_Toggled(object sender, RoutedEventArgs e)
        {
            await ViewModel.SaveAutoCheckAsync(AutoCheckToggle.IsOn);
        }

        private async void TimeoutNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue >= 3)
            {
                await ViewModel.SaveConnectionTimeoutAsync((int)args.NewValue);
            }
        }

        private void ApplyLocalization()
        {
            TitleText.Text = LanguageService.GetString("Settings");
            AppearanceHeader.Text = LanguageService.GetString("Appearance");
            BackdropLabel.Text = LanguageService.GetString("Backdrop");
            LanguageLabel.Text = LanguageService.GetString("Language");
            LoginServersHeader.Text = LanguageService.GetString("LoginServers");
            LoginServersDesc.Text = LanguageService.GetString("ManageLoginServers");
            AddLoginServerButton.Content = LanguageService.GetString("Add");
            ServerMgmtHeader.Text = LanguageService.GetString("ServerManagement");
            AutoCheckLabel.Text = LanguageService.GetString("AutoCheckServerStatus");
            AutoCheckDesc.Text = LanguageService.GetString("AutoCheckServerStatusDesc");
            TimeoutLabel.Text = LanguageService.GetString("ConnectionTimeout");
            TimeoutDesc.Text = LanguageService.GetString("ConnectionTimeoutDesc");
            AboutDesc.Text = LanguageService.GetString("About");
            CopyrightText.Text = LanguageService.GetString("Copyright");
        }

        private async void AddLoginServer_Click(object sender, RoutedEventArgs e)
        {
            var nameBox = new TextBox
            {
                Header = LanguageService.GetString("ServerName"),
                PlaceholderText = LanguageService.GetString("MyLoginServer")
            };
            var addressBox = new TextBox
            {
                Header = LanguageService.GetString("ServerAddress"),
                PlaceholderText = "localhost"
            };
            var portBox = new NumberBox
            {
                Header = LanguageService.GetString("Port"),
                Value = 8081,
                Minimum = 1,
                Maximum = 65535
            };
            var typeCombo = new ComboBox { Header = LanguageService.GetString("Type") };
            typeCombo.Items.Add(LanguageService.GetString("Official"));
            typeCombo.Items.Add(LanguageService.GetString("Local"));
            typeCombo.Items.Add(LanguageService.GetString("ThirdParty"));
            typeCombo.SelectedIndex = 0;

            var panel = new StackPanel { Spacing = 12 };
            panel.Children.Add(nameBox);
            panel.Children.Add(addressBox);
            panel.Children.Add(portBox);
            panel.Children.Add(typeCombo);

            var dialog = new ContentDialog
            {
                Title = LanguageService.GetString("AddLoginServer"),
                Content = panel,
                PrimaryButtonText = LanguageService.GetString("Add"),
                CloseButtonText = LanguageService.GetString("Cancel"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var serverType = typeCombo.SelectedItem?.ToString() switch
                {
                    var s when s == LanguageService.GetString("Local") => LoginServerType.Local,
                    var s when s == LanguageService.GetString("ThirdParty") => LoginServerType.ThirdParty,
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
                var confirmText = string.Format(LanguageService.GetString("ConfirmDeleteServer"), server.Name);
                var confirm = new ContentDialog
                {
                    Title = LanguageService.GetString("DeleteLoginServer"),
                    Content = confirmText,
                    PrimaryButtonText = LanguageService.GetString("Delete"),
                    CloseButtonText = LanguageService.GetString("Cancel"),
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
