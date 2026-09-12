using System;
using System.Diagnostics;
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
        private bool _isLoading;

        public SettingsPage()
        {
            this.InitializeComponent();
            this.Loaded += SettingsPage_Loaded;
        }

        private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _isLoading = true;
                await ViewModel.LoadSettingsAsync();

                BackdropComboBox.Items.Clear();
                BackdropComboBox.Items.Add(LanguageService.GetString("BackdropDefault"));
                BackdropComboBox.Items.Add(LanguageService.GetString("BackdropMica"));
                BackdropComboBox.Items.Add(LanguageService.GetString("BackdropAcrylic"));
                BackdropComboBox.SelectedIndex = ViewModel.SelectedBackdropIndex;

                LanguageComboBox.Items.Clear();
                foreach (var lang in LanguageService.GetAvailableLanguages())
                {
                    LanguageComboBox.Items.Add(LanguageService.GetLanguageDisplayName(lang));
                }
                LanguageComboBox.SelectedIndex = LanguageService.GetLanguageIndex(LanguageService.CurrentLanguage);

                OpacitySlider.Value = ViewModel.Opacity * 100;
                OpacityValueText.Text = $"{(int)(ViewModel.Opacity * 100)}%";

                StoragePathValue.Text = ViewModel.StoragePath;
                LogPathValue.Text = ViewModel.LogPath;

                OfficialAddrBox.Text = ViewModel.OfficialServer.Address;
                OfficialPortBox.Value = ViewModel.OfficialServer.Port;
                OfficialIdBox.Text = ViewModel.OfficialServer.UniqueId;
                OfficialEmailCheck.IsChecked = ViewModel.OfficialServer.SupportsEmail;
                OfficialOfflineCheck.IsChecked = ViewModel.OfficialServer.SupportsOffline;

                ApplyLocalization();
                ShowSection(AppearanceSection, NavAppearance);
                _isLoading = false;
            }
            catch (Exception ex)
            {
                _isLoading = false;
                CrashLogger.LogException("SettingsPage_Loaded", ex);
            }
        }

        private void ShowSection(FrameworkElement section, Button navButton)
        {
            try
            {
                AppearanceSection.Visibility = Visibility.Collapsed;
                ServerMgmtSection.Visibility = Visibility.Collapsed;
                OfficialSection.Visibility = Visibility.Collapsed;
                StorageSection.Visibility = Visibility.Collapsed;
                AboutSection.Visibility = Visibility.Collapsed;

                section.Visibility = Visibility.Visible;

                if (_activeNavButton != null)
                    _activeNavButton.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);

                navButton.Background = (Brush)Application.Current.Resources["SystemControlBackgroundAccentButtonBrush"];
                _activeNavButton = navButton;
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("ShowSection", ex);
            }
        }

        private void NavAppearance_Click(object sender, RoutedEventArgs e) =>
            TryCatch(() => ShowSection(AppearanceSection, NavAppearance));

        private void NavServerMgmt_Click(object sender, RoutedEventArgs e) =>
            TryCatch(() => ShowSection(ServerMgmtSection, NavServerMgmt));

        private void NavOfficial_Click(object sender, RoutedEventArgs e) =>
            TryCatch(() => ShowSection(OfficialSection, NavOfficial));

        private void NavStorage_Click(object sender, RoutedEventArgs e) =>
            TryCatch(() => ShowSection(StorageSection, NavStorage));

        private void NavAbout_Click(object sender, RoutedEventArgs e) =>
            TryCatch(() => ShowSection(AboutSection, NavAbout));

        private async void BackdropComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try { if (!_isLoading && BackdropComboBox.SelectedIndex >= 0) await ViewModel.SaveBackdropAsync(BackdropComboBox.SelectedIndex); }
            catch (Exception ex) { CrashLogger.LogException("BackdropComboBox_SelectionChanged", ex); }
        }

        private async void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_isLoading) return;
                if (LanguageComboBox.SelectedIndex >= 0)
                {
                    var langs = LanguageService.GetAvailableLanguages();
                    var selectedLang = langs[LanguageComboBox.SelectedIndex];
                    if (selectedLang != LanguageService.CurrentLanguage)
                    {
                        await LanguageService.SetLanguageAsync(selectedLang);
                        ApplyLocalization();
                        StatusInfoBar.Message = LanguageService.GetString("LanguageChanged");
                        StatusInfoBar.IsOpen = true;
                    }
                }
            }
            catch (Exception ex) { CrashLogger.LogException("LanguageComboBox_SelectionChanged", ex); }
        }

        private async void OpacitySlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            try
            {
                if (_isLoading) return;
                var opacity = e.NewValue / 100.0;
                OpacityValueText.Text = $"{(int)e.NewValue}%";
                await ViewModel.SaveOpacityAsync(opacity);
            }
            catch (Exception ex) { CrashLogger.LogException("OpacitySlider_ValueChanged", ex); }
        }

        private async void AutoCheckToggle_Toggled(object sender, RoutedEventArgs e) =>
            TryCatch(async () => { if (!_isLoading) await ViewModel.SaveAutoCheckAsync(AutoCheckToggle.IsOn); });

        private async void TimeoutNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            try { if (!_isLoading && args.NewValue >= 3) await ViewModel.SaveConnectionTimeoutAsync((int)args.NewValue); }
            catch (Exception ex) { CrashLogger.LogException("TimeoutNumberBox_ValueChanged", ex); }
        }

        private async void SaveOfficialButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var server = new OfficialServer
                {
                    Address = OfficialAddrBox.Text?.Trim() ?? "server.talkift.com",
                    Port = (int)OfficialPortBox.Value,
                    SupportsEmail = OfficialEmailCheck.IsChecked == true,
                    SupportsOffline = OfficialOfflineCheck.IsChecked == true
                };
                await ViewModel.SaveOfficialServerAsync(server);
                StatusInfoBar.Message = LanguageService.GetString("SettingsSaved");
                StatusInfoBar.IsOpen = true;
            }
            catch (Exception ex) { CrashLogger.LogException("SaveOfficialButton_Click", ex); }
        }

        private async void ChangeStoragePath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FolderPicker();
                picker.FileTypeFilter.Add("*");
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.CurrentWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
                var folder = await picker.PickSingleFolderAsync();
                if (folder != null)
                {
                    await ViewModel.SaveStoragePathAsync(folder.Path);
                    StoragePathValue.Text = folder.Path;
                    StatusInfoBar.Message = LanguageService.GetString("SettingsSaved");
                    StatusInfoBar.IsOpen = true;
                }
            }
            catch (Exception ex) { CrashLogger.LogException("ChangeStoragePath_Click", ex); }
        }

        private void OpenLogFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var logPath = StorageService.LogsDir;
                if (System.IO.Directory.Exists(logPath))
                    Process.Start("explorer.exe", logPath);
            }
            catch (Exception ex) { CrashLogger.LogException("OpenLogFolder_Click", ex); }
        }

        private void ApplyLocalization()
        {
            try
            {
                TitleText.Text = LanguageService.GetString("Settings");
                NavAppearanceText.Text = LanguageService.GetString("Appearance");
                NavServerMgmtText.Text = LanguageService.GetString("ServerManagement");
                NavOfficialText.Text = LanguageService.GetString("OfficialServer");
                NavStorageText.Text = LanguageService.GetString("Storage");
                NavAboutText.Text = LanguageService.GetString("About");

                AppearanceHeader.Text = LanguageService.GetString("Appearance");
                AppearanceDesc.Text = LanguageService.GetString("CustomizeAppearance");
                BackdropLabel.Text = LanguageService.GetString("Backdrop");
                OpacityLabel.Text = LanguageService.GetString("WindowOpacity");
                LanguageLabel.Text = LanguageService.GetString("Language");

                ServerMgmtHeader.Text = LanguageService.GetString("ServerManagement");
                ServerMgmtDesc.Text = LanguageService.GetString("ConfigureServerSettings");
                AutoCheckLabel.Text = LanguageService.GetString("AutoCheckServerStatus");
                AutoCheckDesc.Text = LanguageService.GetString("AutoCheckServerStatusDesc");
                TimeoutLabel.Text = LanguageService.GetString("ConnectionTimeout");
                TimeoutDesc.Text = LanguageService.GetString("ConnectionTimeoutDesc");

                OfficialHeader.Text = LanguageService.GetString("OfficialServer");
                OfficialDesc.Text = LanguageService.GetString("OfficialServerDesc");
                ((TextBlock)OfficialAddrBox.Header).Text = LanguageService.GetString("ServerAddress");
                OfficialAddrBox.PlaceholderText = "server.talkift.com";
                ((TextBlock)OfficialPortBox.Header).Text = LanguageService.GetString("Port");
                ((TextBlock)OfficialIdBox.Header).Text = LanguageService.GetString("ServerName");
                OfficialEmailCheck.Content = LanguageService.GetString("LoginWithEmail");
                OfficialOfflineCheck.Content = LanguageService.GetString("OfflineLogin");
                SaveOfficialButton.Content = LanguageService.GetString("Save");

                StorageHeader.Text = LanguageService.GetString("Storage");
                StorageDesc.Text = LanguageService.GetString("StorageDesc");
                StoragePathLabel.Text = LanguageService.GetString("DataStoragePath");
                LogPathLabel.Text = LanguageService.GetString("LogStoragePath");
                ChangeStoragePathButton.Content = LanguageService.GetString("Change");
                OpenLogFolderButton.Content = LanguageService.GetString("Open");

                AboutDesc.Text = LanguageService.GetString("About");
                CopyrightText.Text = LanguageService.GetString("Copyright");
            }
            catch (Exception ex) { CrashLogger.LogException("SettingsPage.ApplyLocalization", ex); }
        }

        private void TryCatch(Action action)
        {
            try { action(); }
            catch (Exception ex) { CrashLogger.LogException("SettingsPage.TryCatch", ex); }
        }
    }
}
