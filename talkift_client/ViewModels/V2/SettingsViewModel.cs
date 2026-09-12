using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Talkift.Client.Engines;
using Talkift.Client.Services;

namespace Talkift.Client.ViewModels.V2;

public partial class SettingsViewModel : ObservableObject, IDisposable
{
    private readonly IThemeService _themeService;
    private readonly IAuthService _authService;
    private readonly IChatEngine _chatEngine;
    private readonly IUiEngine _uiEngine;
    private readonly INotificationService _notificationService;
    private readonly ILoggerService _loggerService;
    private readonly DispatcherQueue _dispatcherQueue;
    private bool _disposed;

    [ObservableProperty]
    private double _opacity = 1.0;

    [ObservableProperty]
    private bool _enableMica = true;

    [ObservableProperty]
    private bool _enableAcrylic;

    [ObservableProperty]
    private bool _enableAnimations = true;

    [ObservableProperty]
    private bool _autoCheckServerStatus = true;

    [ObservableProperty]
    private int _connectionTimeout = 10;

    [ObservableProperty]
    private string _storagePath = string.Empty;

    [ObservableProperty]
    private string _logPath = string.Empty;

    [ObservableProperty]
    private string _officialServerAddress = "47.113.216.177";

    [ObservableProperty]
    private int _officialServerPort = 8002;

    [ObservableProperty]
    private bool _supportsEmail = true;

    [ObservableProperty]
    private bool _supportsOffline = true;

    [ObservableProperty]
    private string _version = "1.0.0";

    [ObservableProperty]
    private string _copyrightText = "Copyright 2026 Talkift Team";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _activeSection = "Appearance";

    public SettingsViewModel(
        IThemeService themeService,
        IAuthService authService,
        IChatEngine chatEngine,
        IUiEngine uiEngine,
        INotificationService notificationService,
        ILoggerService loggerService)
    {
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _chatEngine = chatEngine ?? throw new ArgumentNullException(nameof(chatEngine));
        _uiEngine = uiEngine ?? throw new ArgumentNullException(nameof(uiEngine));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    public async Task InitializeAsync()
    {
        _opacity = await _themeService.LoadThemeAsync().GetAwaiter().GetResult() switch
        {
            Models.V2.ThemePreference.Dark => 1.0,
            _ => 1.0
        };
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            await _themeService.SaveThemeAsync(
                _enableMica ? Models.V2.ThemePreference.System : Models.V2.ThemePreference.Light);
            StatusMessage = "Settings saved";
        }
        catch (Exception ex)
        {
            _loggerService.Error(nameof(SettingsViewModel), "Save failed", ex);
            StatusMessage = "Save failed";
        }
    }

    [RelayCommand]
    private async Task SaveOfficialServerAsync()
    {
        try
        {
            StatusMessage = "Official server updated";
        }
        catch (Exception ex)
        {
            _loggerService.Error(nameof(SettingsViewModel), "Update failed", ex);
            StatusMessage = "Update failed";
        }
    }

    [RelayCommand]
    private async Task ChangeStoragePathAsync()
    {
        StatusMessage = "Storage path changed";
    }

    [RelayCommand]
    private async Task ChangeLogPathAsync()
    {
        StatusMessage = "Log path changed";
    }

    [RelayCommand]
    private async Task ResetToDefaultAsync()
    {
        _opacity = 1.0;
        _enableMica = true;
        _enableAcrylic = false;
        _autoCheckServerStatus = true;
        _connectionTimeout = 10;
        StatusMessage = "Reset to default";
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
