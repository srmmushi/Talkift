using System;
using System.Threading;
using System.Threading.Tasks;

namespace Talkift.Client.Services;

public sealed class AuthResult
{
    public bool Success { get; init; }
    public string? UserId { get; init; }
    public string? Username { get; init; }
    public string? Email { get; init; }
    public string? Token { get; init; }
    public string? Error { get; init; }
    public string RegisterMethod { get; init; } = "local";
}

public interface IAuthService : IDisposable
{
    bool IsLoggedIn { get; }
    string? CurrentUserId { get; }
    string? CurrentUsername { get; }
    string? CurrentEmail { get; }
    string? CurrentToken { get; }
    string? CurrentRegisterMethod { get; }

    event EventHandler<bool>? LoginStateChanged;

    Task<AuthResult> LoginWithUsernameAsync(string serverAddress, int port, string username, string password, CancellationToken ct = default);
    Task<AuthResult> LoginWithEmailAsync(string serverAddress, int port, string email, string password, CancellationToken ct = default);
    Task<AuthResult> RegisterAsync(string serverAddress, int port, string username, string email, string password, CancellationToken ct = default);
    Task<AuthResult> OfflineLoginAsync(string username, CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
    Task<AuthResult> LoadSavedCredentialsAsync(CancellationToken ct = default);
    Task SaveCredentialsAsync(string username, string? email, string password, string method, CancellationToken ct = default);
    Task ClearCredentialsAsync(CancellationToken ct = default);
}
