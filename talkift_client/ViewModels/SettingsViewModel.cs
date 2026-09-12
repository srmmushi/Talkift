using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Talkift.Client.Models;
using Talkift.Client.Services;

namespace Talkift.Client.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly StorageService _storage = new();

        [ObservableProperty]
        private string _appVersion = "1.0.0";

        [ObservableProperty]
        private int _selectedBackdropIndex;

        [ObservableProperty]
        private int _selectedLanguageIndex;

        [ObservableProperty]
        private bool _autoCheckServerStatus = true;

        [ObservableProperty]
        private int _connectionTimeout = 10;

        [ObservableProperty]
        private double _opacity = 1.0;

        [ObservableProperty]
        private string _storagePath = string.Empty;

        [ObservableProperty]
        private string _logPath = string.Empty;

        [ObservableProperty]
        private OfficialServer _officialServer = new();

        public string[] BackdropOptions => new[]
        {
            LanguageService.GetString("BackdropDefault"),
            LanguageService.GetString("BackdropMica"),
            LanguageService.GetString("BackdropAcrylic")
        };

        public string[] LanguageOptions => new[]
        {
            "English",
            "\u4e2d\u6587"
        };

        public ObservableCollection<Server> Servers { get; } = new();

        public async Task LoadSettingsAsync()
        {
            Servers.Clear();
            var servers = await _storage.LoadAsync<List<Server>>("servers");
            if (servers != null)
            {
                foreach (var s in servers)
                    Servers.Add(s);
            }

            SelectedBackdropIndex = (int)BackdropService.CurrentBackdrop;
            SelectedLanguageIndex = LanguageService.CurrentLanguage switch
            {
                "zh" => 1,
                _ => 0
            };

            var autoCheck = await _storage.LoadAsync<bool?>("auto_check_status");
            AutoCheckServerStatus = autoCheck ?? true;

            var timeout = await _storage.LoadAsync<int?>("connection_timeout");
            ConnectionTimeout = timeout ?? 10;

            var opacity = await _storage.LoadAsync<double?>("opacity");
            Opacity = opacity ?? 1.0;

            StoragePath = StorageService.DataDir;
            LogPath = StorageService.LogsDir;

            var official = await _storage.LoadAsync<OfficialServer>("official_server");
            if (official != null)
                OfficialServer = official;
        }

        public async Task SaveBackdropAsync(int index)
        {
            var type = (BackdropType)index;
            await BackdropService.SetBackdropAsync(type);

            var mainWindow = App.CurrentWindow as MainWindow;
            if (mainWindow != null)
            {
                BackdropService.ApplyBackdrop(mainWindow, type);
            }
        }

        public async Task SaveLanguageAsync(int index)
        {
            var lang = index switch
            {
                1 => "zh",
                _ => "en"
            };
            await LanguageService.SetLanguageAsync(lang);
        }

        public async Task SaveOpacityAsync(double opacity)
        {
            Opacity = opacity;
            await BackdropService.SetOpacityAsync(opacity);
            var mainWindow = App.CurrentWindow as MainWindow;
            if (mainWindow?.RootGrid != null)
            {
                BackdropService.ApplyOpacity(mainWindow.RootGrid, opacity);
            }
        }

        public async Task SaveAutoCheckAsync(bool value)
        {
            AutoCheckServerStatus = value;
            await _storage.SaveAsync("auto_check_status", value);
        }

        public async Task SaveConnectionTimeoutAsync(int value)
        {
            ConnectionTimeout = value;
            await _storage.SaveAsync("connection_timeout", value);
        }

        public async Task SaveStoragePathAsync(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && System.IO.Directory.Exists(path))
            {
                StoragePath = path;
                StorageService.SetDataDirectory(path);
                await _storage.SaveAsync("storage_path", path);
            }
        }

        public async Task SaveOfficialServerAsync(OfficialServer server)
        {
            OfficialServer = server;
            await _storage.SaveAsync("official_server", server);
        }
    }
}
