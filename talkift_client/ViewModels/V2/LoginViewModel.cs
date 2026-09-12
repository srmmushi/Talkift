using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Talkift.Client.Engines;
using Talkift.Client.Services;
using Talkift.Client.Models.V2;

namespace Talkift.Client.ViewModels.V2;

public partial class LoginViewModel : ObservableObject, IDisposable
{
    private readonly IAuthService _authService;
    private readonly IChatEngine _chatEngine;
    private readonly IThemeService _themeService;
    private readonly ILoggerService _loggerService;
    private readonly DispatcherQueue _dispatcherQueue;
    private bool _disposed;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private bool _isPasswordVisible;

    [ObservableProperty]
    private bool _isLoginMode = true;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorText = string.Empty;

    [ObservableProperty]
    private bool _isOfflineLogin;

    [ObservableProperty]
    private string _serverAddress = "47.113.216.177";

    [ObservableProperty]
    private int _serverPort = 8002;

    [ObservableProperty]
    private string? _savedUsername;

    [ObservableProperty]
    private string? _savedEmail;

    [ObservableProperty]
    private string _loginMethod = "local";

    public LoginViewModel(
        IAuthService authService,
        IChatEngine chatEngine,
        IThemeService themeService,
        ILoggerService loggerService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _chatEngine = chatEngine ?? throw new ArgumentNullException(nameof(chatEngine));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    public async Task InitializeAsync()
    {
        var creds = await _authService.LoadSavedCredentialsAsync();
        if (creds.Success)
        {
            _savedUsername = creds.Username;
            _savedEmail = creds.Email;
            _loginMethod = creds.RegisterMethod;
        }
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(_username) && !IsOfflineLogin)
        {
            ErrorText = "Username is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(_password))
        {
            ErrorText = "Password is required";
            return;
        }

        _isLoading = true;
        _errorText = string.Empty;

        try
        {
            if (IsOfflineLogin)
            {
                var result = await _authService.OfflineLoginAsync(_username, _cts.Token);
                if (result.Success)
                {
                    _isLoading = false;
                }
                else
                {
                    ErrorText = result.Error ?? "Offline login failed";
                    _isLoading = false;
                }
            }
            else if (_loginMethod == "email")
            {
                var result = await _authService.LoginWithEmailAsync(
                    _serverAddress, _serverPort, _email, _password, _cts.Token);
                if (result.Success)
                {
                    _isLoading = false;
                }
                else
                {
                    ErrorText = result.Error ?? "Login failed";
                    _isLoading = false;
                }
            }
            else
            {
                var result = await _authService.LoginWithUsernameAsync(
                    _serverAddress, _serverPort, _username, _password, _cts.Token);
                if (result.Success)
                {
                    _isLoading = false;
                }
                else
                {
                    ErrorText = result.Error ?? "Login failed";
                    _isLoading = false;
                }
            }
        }
        catch (Exception ex)
        {
            _loggerService.Error(nameof(LoginViewModel), "Login failed", ex);
            ErrorText = "An unexpected error occurred";
            _isLoading = false;
        }
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(_username))
        {
            ErrorText = "Username is required";
            return;
        }
        if (string.IsNullOrWhiteSpace(_email))
        {
            ErrorText = "Email is required";
            return;
        }
        if (string.IsNullOrWhiteSpace(_password))
        {
            ErrorText = "Password is required";
            return;
        }
        if (_password != _confirmPassword)
        {
            ErrorText = "Passwords do not match";
            return;
        }

        _isLoading = true;
        _errorText = string.Empty;

        try
        {
            var result = await _authService.RegisterAsync(
                _serverAddress, _serverPort, _username, _email, _password, _cts.Token);
            if (result.Success)
            {
                _isLoading = false;
            }
            else
            {
                ErrorText = result.Error ?? "Registration failed";
                _isLoading = false;
            }
        }
        catch (Exception ex)
        {
            _loggerService.Error(nameof(LoginViewModel), "Register failed", ex);
            ErrorText = "An unexpected error occurred";
            _isLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleMode()
    {
        _isLoginMode = !_isLoginMode;
        _errorText = string.Empty;
    }

    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        _isPasswordVisible = !_isPasswordVisible;
    }

    [RelayCommand]
    private async Task LoadOfflineAsync()
    {
        _isOfflineLogin = true;
        var result = await _authService.OfflineLoginAsync(_username, _cts.Token);
        if (!result.Success)
        {
            ErrorText = result.Error ?? "Offline login failed";
        }
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
