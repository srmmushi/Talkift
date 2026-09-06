using System;
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

        public ObservableCollection<LoginServer> LoginServers { get; } = new();

        [RelayCommand]
        private async Task LoadSettingsAsync()
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
        }

        [RelayCommand]
        private async Task AddLoginServerAsync(LoginServer server)
        {
            LoginServers.Add(server);
            await SaveLoginServersAsync();
        }

        [RelayCommand]
        private async Task RemoveLoginServerAsync(LoginServer server)
        {
            LoginServers.Remove(server);
            await SaveLoginServersAsync();
        }

        private async Task SaveLoginServersAsync()
        {
            await _storage.SaveAsync("login_servers", LoginServers);
        }
    }
}
