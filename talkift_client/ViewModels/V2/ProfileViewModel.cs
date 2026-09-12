using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Talkift.Client.Engines;
using Talkift.Client.Models.V2;
using Talkift.Client.Services;

namespace Talkift.Client.ViewModels.V2;

public partial class ProfileViewModel : ObservableObject, IDisposable
{
    private readonly IAuthService _authService;
    private readonly IThemeService _themeService;
    private readonly IChatEngine _chatEngine;
    private readonly ILoggerService _loggerService;
    private bool _disposed;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private UserProfileModel _profile = new();

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _bio = string.Empty;

    [ObservableProperty]
    private string _signature = string.Empty;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private string _avatarUrl = string.Empty;

    public ProfileViewModel(
        IAuthService authService,
        IThemeService themeService,
        IChatEngine chatEngine,
        ILoggerService loggerService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _chatEngine = chatEngine ?? throw new ArgumentNullException(nameof(chatEngine));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
    }

    public async Task InitializeAsync()
    {
        try
        {
            Profile = new UserProfileModel
            {
                Id = _authService.CurrentUserId ?? string.Empty,
                Username = _authService.CurrentUsername ?? string.Empty,
                Email = _authService.CurrentEmail ?? string.Empty,
                Status = "online",
                RegisterMethod = _authService.CurrentRegisterMethod ?? "local"
            };
        }
        catch (Exception ex)
        {
            _loggerService.Error(nameof(ProfileViewModel), "Init failed", ex);
        }
    }

    [RelayCommand]
    private async Task UpdateProfileAsync()
    {
        try
        {
            StatusMessage = "Profile updated";
            IsEditMode = false;
        }
        catch (Exception ex)
        {
            _loggerService.Error(nameof(ProfileViewModel), "Update failed", ex);
            StatusMessage = "Update failed";
        }
    }

    [RelayCommand]
    private void StartEdit()
    {
        IsEditMode = true;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditMode = false;
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private async Task ChangeAvatarAsync()
    {
        StatusMessage = "Avatar changed";
    }

    [RelayCommand]
    private async Task ChangeStatusAsync(string status)
    {
        Profile.Status = status;
        await _chatEngine.SendTypingAsync(
            _authService.CurrentUsername ?? string.Empty,
            status != "online", _cts.Token);
        StatusMessage = $"Status: {status}";
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _cts?.Cancel();
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
