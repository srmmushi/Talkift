using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Talkift.Client.Models;
using Talkift.Client.Services;

namespace Talkift.Client.ViewModels
{
    public partial class ServerListViewModel : ViewModelBase
    {
        private readonly StorageService _storage = new();
        private readonly ServerService _serverService = new();
        private readonly CredentialService _credentialService = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private InfoBarSeverity _infoBarSeverity;

        [ObservableProperty]
        private string _infoBarMessage = string.Empty;

        [ObservableProperty]
        private bool _isInfoBarOpen;

        public ObservableCollection<Server> Servers { get; } = new();

        public event Func<Server, Task>? ServerClicked;

        public async Task LoadServersAsync()
        {
            IsLoading = true;
            try
            {
                Servers.Clear();
                var servers = await _storage.LoadAsync<List<Server>>("servers");
                if (servers != null)
                {
                    foreach (var s in servers)
                    {
                        Servers.Add(s);
                    }
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task AddServerAsync(Server server)
        {
            Servers.Add(server);
            await SaveServersAsync();
        }

        public async Task RemoveServerAsync(Server server)
        {
            Servers.Remove(server);
            await SaveServersAsync();
            await _credentialService.DeleteCredentialAsync(server.Id);
        }

        public async Task EditServerAsync(Server server)
        {
            await SaveServersAsync();
        }

        private async Task SaveServersAsync()
        {
            await _storage.SaveAsync("servers", Servers);
        }

        public async Task CheckServerStatusAsync(Server server)
        {
            server.IsOnline = await _serverService.TestConnectionAsync(server.Address, server.Port);
        }

        public void ShowInfoBar(InfoBarSeverity severity, string message)
        {
            InfoBarSeverity = severity;
            InfoBarMessage = message;
            IsInfoBarOpen = true;
        }

        public async Task<StoredCredential?> TryGetCredentialAsync(Server server)
        {
            return await _credentialService.LoadCredentialAsync(server.Id);
        }

        public async Task HandleServerClickAsync(Server server, MainViewModel mainViewModel)
        {
            await CheckServerStatusAsync(server);

            if (!server.IsOnline)
            {
                ShowInfoBar(InfoBarSeverity.Error, LanguageService.GetString("ServerOffline"));
                return;
            }

            var credential = await TryGetCredentialAsync(server);
            if (credential != null && !string.IsNullOrEmpty(credential.Token))
            {
                mainViewModel.AuthService.SetCredentials(
                    credential.Token,
                    string.Empty,
                    credential.Username,
                    credential.RegisterMethod);

                mainViewModel.SelectServer(server);
                mainViewModel.OnLoginSuccess(credential.Username, credential.RegisterMethod);
                return;
            }

            mainViewModel.SelectServer(server);
        }
    }
}
