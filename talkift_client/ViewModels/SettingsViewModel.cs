using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        private int _selectedTabIndex;

        [ObservableProperty]
        private int _selectedBackdropIndex;

        [ObservableProperty]
        private int _selectedLanguageIndex;

        [ObservableProperty]
        private bool _autoCheckServerStatus = true;

        [ObservableProperty]
        private int _connectionTimeout = 10;

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

        public ObservableCollection<LoginServer> LoginServers { get; } = new();

        public async Task LoadSettingsAsync()
        {
            LoginServers.Clear();
            var servers = await _storage.LoadAsync<List<LoginServer>>("login_servers");
            if (servers != null)
            {
                foreach (var s in servers)
                {
                    LoginServers.Add(s);
                }
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
        }

        public async Task AddLoginServerAsync(LoginServer server)
        {
            LoginServers.Add(server);
            await SaveLoginServersAsync();
        }

        public async Task RemoveLoginServerAsync(LoginServer server)
        {
            LoginServers.Remove(server);
            await SaveLoginServersAsync();
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

        private async Task SaveLoginServersAsync()
        {
            await _storage.SaveAsync("login_servers", LoginServers);
        }
    }
}
