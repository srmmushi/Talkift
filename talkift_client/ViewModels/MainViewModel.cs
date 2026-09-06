using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Talkift.Client.Models;
using Talkift.Client.Services;

namespace Talkift.Client.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly AuthService _authService = new();
        private readonly CredentialService _credentialService = new();

        public AuthService AuthService => _authService;
        public CredentialService CredentialService => _credentialService;

        [ObservableProperty]
        private string _title = "Talkift";

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private Server? _selectedServer;

        [ObservableProperty]
        private PageType _currentPage = PageType.ServerList;

        [ObservableProperty]
        private bool _isLoggedIn;

        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private string _currentUsername = string.Empty;

        [ObservableProperty]
        private string _currentRegisterMethod = string.Empty;

        [ObservableProperty]
        private string _statusBarText = string.Empty;

        partial void OnIsLoggedInChanged(bool value)
        {
            if (value)
            {
                StatusBarText = CurrentUsername;
            }
            else
            {
                StatusBarText = string.Empty;
            }
        }

        [RelayCommand]
        private void NavigateTo(PageType page)
        {
            CurrentPage = page;
        }

        [RelayCommand]
        private void SelectServer(Server server)
        {
            SelectedServer = server;
        }

        [RelayCommand]
        private void Logout()
        {
            _authService.Logout();
            IsLoggedIn = false;
            CurrentUser = null;
            CurrentUsername = string.Empty;
            CurrentRegisterMethod = string.Empty;
            SelectedServer = null;
            CurrentPage = PageType.ServerList;
        }

        public void OnLoginSuccess(string username, string registerMethod)
        {
            IsLoggedIn = true;
            CurrentUsername = username;
            CurrentRegisterMethod = registerMethod;
            CurrentUser = new User
            {
                Username = username,
                RegisterMethod = registerMethod
            };
        }

        public static string GetRegisterMethodGlyph(string method)
        {
            return method?.ToLower() switch
            {
                "official" => "\uE716",
                "local" => "\uE77B",
                "third_party" => "\uE710",
                _ => "\uE716"
            };
        }

        public static Windows.UI.Color GetRegisterMethodColor(string method)
        {
            return method?.ToLower() switch
            {
                "official" => Windows.UI.Color.FromArgb(255, 0, 120, 215),
                "local" => Windows.UI.Color.FromArgb(255, 16, 137, 64),
                "third_party" => Windows.UI.Color.FromArgb(255, 136, 23, 152),
                _ => Windows.UI.Color.FromArgb(255, 0, 120, 215)
            };
        }
    }
}
