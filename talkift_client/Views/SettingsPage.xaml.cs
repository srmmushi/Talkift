using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Talkift.Client.Models;
using Talkift.Client.Services;
using Talkift.Client.ViewModels;

namespace Talkift.Client.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; } = new();

        private Button? _activeNavButton;

        public SettingsPage()
        {
            this.InitializeComponent();
            this.Loaded += SettingsPage_Loaded;
        }

        private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
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
                ShowSection(AppearanceSection, NavAppearance);
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("SettingsPage_Loaded", ex);
            }
        }

        private void ShowSection(FrameworkElement section, Button navButton)
        {
            try
            {
                AppearanceSection.Visibility = Visibility.Collapsed;
                LoginServersSection.Visibility = Visibility.Collapsed;
                ServerMgmtSection.Visibility = Visibility.Collapsed;
                AboutSection.Visibility = Visibility.Collapsed;

                section.Visibility = Visibility.Visible;

                if (_activeNavButton != null)
                {
                    _activeNavButton.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                }

                navButton.Background = (Brush)Application.Current.Resources["SystemControlBackgroundAccentButtonBrush"];
                _activeNavButton = navButton;
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("ShowSection", ex);
            }
        }

        private void NavAppearance_Click(object sender, RoutedEventArgs e)
        {
            try { ShowSection(AppearanceSection, NavAppearance); }
            catch (Exception ex) { CrashLogger.LogException("NavAppearance_Click", ex); }
        }

        private void NavLoginServers_Click(object sender, RoutedEventArgs e)
        {
            try { ShowSection(LoginServersSection, NavLoginServers); }
            catch (Exception ex) { CrashLogger.LogException("NavLoginServers_Click", ex); }
        }

        private void NavServerMgmt_Click(object sender, RoutedEventArgs e)
        {
            try { ShowSection(ServerMgmtSection, NavServerMgmt); }
            catch (Exception ex) { CrashLogger.LogException("NavServerMgmt_Click", ex); }
        }

        private void NavAbout_Click(object sender, RoutedEventArgs e)
        {
            try { ShowSection(AboutSection, NavAbout); }
            catch (Exception ex) { CrashLogger.LogException("NavAbout_Click", ex); }
        }

        private async void BackdropComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (BackdropComboBox.SelectedIndex >= 0)
                {
                    await ViewModel.SaveBackdropAsync(BackdropComboBox.SelectedIndex);
                }
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("BackdropComboBox_SelectionChanged", ex);
            }
        }

        private async void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (LanguageComboBox.SelectedIndex >= 0 && LanguageComboBox.SelectedIndex != ViewModel.SelectedLanguageIndex)
                {
                    await ViewModel.SaveLanguageAsync(LanguageComboBox.SelectedIndex);
                    ApplyLocalization();

                    StatusInfoBar.Message = LanguageService.GetString("LanguageChanged");
                    StatusInfoBar.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("LanguageComboBox_SelectionChanged", ex);
            }
        }

        private async void AutoCheckToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                await ViewModel.SaveAutoCheckAsync(AutoCheckToggle.IsOn);
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("AutoCheckToggle_Toggled", ex);
            }
        }

        private async void TimeoutNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            try
            {
                if (args.NewValue >= 3)
                {
                    await ViewModel.SaveConnectionTimeoutAsync((int)args.NewValue);
                }
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("TimeoutNumberBox_ValueChanged", ex);
            }
        }

        private void ApplyLocalization()
        {
            try
            {
                TitleText.Text = LanguageService.GetString("Settings");
                NavAppearanceText.Text = LanguageService.GetString("Appearance");
                NavLoginServersText.Text = LanguageService.GetString("LoginServers");
                NavServerMgmtText.Text = LanguageService.GetString("ServerManagement");
                NavAboutText.Text = LanguageService.GetString("About");

                AppearanceHeader.Text = LanguageService.GetString("Appearance");
                AppearanceDesc.Text = "Customize the look and feel of Talkift";
                BackdropLabel.Text = LanguageService.GetString("Backdrop");
                BackdropDesc.Text = "Choose the background effect for the application window";
                LanguageLabel.Text = LanguageService.GetString("Language");
                LanguageDesc.Text = "Select your preferred language for the interface";

                LoginServersHeader.Text = LanguageService.GetString("LoginServers");
                LoginServersDesc.Text = LanguageService.GetString("ManageLoginServers");
                AddLoginServerButton.Content = LanguageService.GetString("Add");

                ServerMgmtHeader.Text = LanguageService.GetString("ServerManagement");
                ServerMgmtDesc.Text = "Configure server connection and status settings";
                AutoCheckLabel.Text = LanguageService.GetString("AutoCheckServerStatus");
                AutoCheckDesc.Text = LanguageService.GetString("AutoCheckServerStatusDesc");
                TimeoutLabel.Text = LanguageService.GetString("ConnectionTimeout");
                TimeoutDesc.Text = LanguageService.GetString("ConnectionTimeoutDesc");

                AboutDesc.Text = LanguageService.GetString("About");
                CopyrightText.Text = LanguageService.GetString("Copyright");
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("SettingsPage.ApplyLocalization", ex);
            }
        }

        private async void AddLoginServer_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                CrashLogger.LogException("AddLoginServer_Click", ex);
            }
        }

        private async void DeleteLoginServer_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                CrashLogger.LogException("DeleteLoginServer_Click", ex);
            }
        }
    }
}
