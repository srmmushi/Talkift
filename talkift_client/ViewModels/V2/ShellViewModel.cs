using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Talkift.Client.Engines;
using Talkift.Client.Models.V2;
using Talkift.Client.Services;

namespace Talkift.Client.ViewModels.V2;

public partial class ShellViewModel : ObservableObject
{
    private readonly IChatEngine _chatEngine;
    private readonly IUiEngine _uiEngine;
    private readonly IAuthService _authService;
    private readonly IThemeService _themeService;
    private readonly INotificationService _notificationService;
    private readonly ILoggerService _loggerService;
    private readonly DispatcherQueue _dispatcherQueue;
    private bool _disposed;

    [ObservableProperty]
    private string _appTitle = "Talkift";

    [ObservableProperty]
    private string _currentPage = "ServerList";

    [ObservableProperty]
    private ObservableCollection<ConversationModel> _conversations = new();

    [ObservableProperty]
    private ConversationModel? _selectedConversation;

    [ObservableProperty]
    private string _connectionStatus = "Disconnected";

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _userAvatar = string.Empty;

    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private int _unreadCount;

    [ObservableProperty]
    private double _windowOpacity = 1.0;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isDarkMode;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private object? _pageContent;

    public ShellViewModel(
        IChatEngine chatEngine,
        IUiEngine uiEngine,
        IAuthService authService,
        IThemeService themeService,
        INotificationService notificationService,
        ILoggerService loggerService)
    {
        _chatEngine = chatEngine ?? throw new ArgumentNullException(nameof(chatEngine));
        _uiEngine = uiEngine ?? throw new ArgumentNullException(nameof(uiEngine));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        _chatEngine.ConnectionStateChanged += OnConnectionStateChanged;
        _themeService.ThemeChanged += OnThemeChanged;
        _authService.LoginStateChanged += OnLoginStateChanged;

        InitializeAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeAsync()
    {
        try
        {
            _isLoading = true;

            var theme = await _themeService.LoadThemeAsync();
            _isDarkMode = theme == Talkift.Client.Services.ThemePreference.Dark ||
                         (theme == Talkift.Client.Services.ThemePreference.System && _isDarkMode);

            var creds = await _authService.LoadSavedCredentialsAsync();
            if (creds.Success && creds.Username != null)
            {
                _username = creds.Username;
                _isLoggedIn = true;
            }

            _isLoading = false;
        }
        catch (Exception ex)
        {
            _loggerService.Error(nameof(ShellViewModel), "Init failed", ex);
            _isLoading = false;
        }
    }

    private void OnConnectionStateChanged(object? sender, ConnectionStateChangedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            IsConnected = e.NewState == ConnectionState.Connected;
            ConnectionStatus = e.NewState.ToString();
            _statusMessage = e.Reason ?? string.Empty;

            if (!IsConnected)
            {
                _notificationService.ShowWarningAsync("Connection lost. Reconnecting...").GetAwaiter().GetResult();
            }
        });
    }

    private void OnThemeChanged(object? sender, bool isDark)
    {
        _dispatcherQueue.TryEnqueue(() => IsDarkMode = isDark);
    }

    private void OnLoginStateChanged(object? sender, bool isLoggedIn)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            IsLoggedIn = isLoggedIn;
            if (isLoggedIn)
            {
                _username = _authService.CurrentUsername ?? string.Empty;
                _isLoggedIn = true;
            }
        });
    }

    [RelayCommand]
    private async Task NavigateToAsync(string page)
    {
        CurrentPage = page;
        await _uiEngine.NavigateToAsync(page);
    }

    [RelayCommand]
    private async Task NavigateBackAsync()
    {
        await _uiEngine.NavigateBackAsync();
    }

    [RelayCommand]
    private async Task ToggleThemeAsync()
    {
        var newTheme = IsDarkMode
            ? Talkift.Client.Services.ThemePreference.Light
            : Talkift.Client.Services.ThemePreference.Dark;
        await _themeService.SaveThemeAsync(newTheme);
    }

    [RelayCommand]
    private async Task ShowNotificationAsync()
    {
        await _notificationService.ShowInfoAsync("Welcome to Talkift!");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _chatEngine.ConnectionStateChanged -= OnConnectionStateChanged;
        _themeService.ThemeChanged -= OnThemeChanged;
        _authService.LoginStateChanged -= OnLoginStateChanged;
        GC.SuppressFinalize(this);
    }
}
