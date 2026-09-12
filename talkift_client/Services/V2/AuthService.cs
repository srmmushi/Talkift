using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Talkift.Client.Services.V2;

internal sealed class AuthResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

public sealed class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private bool _isLoggedIn;
    private string? _userId;
    private string? _username;
    private string? _email;
    private string? _token;
    private string? _registerMethod;
    private bool _disposed;

    public bool IsLoggedIn => _isLoggedIn;
    public string? CurrentUserId => _userId;
    public string? CurrentUsername => _username;
    public string? CurrentEmail => _email;
    public string? CurrentToken => _token;
    public string? CurrentRegisterMethod => _registerMethod;

    public event EventHandler<bool>? LoginStateChanged;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthService()
    {
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
    }

    public async Task<AuthResult> LoginWithUsernameAsync(string serverAddress, int port, string username, string password, CancellationToken ct = default)
    {
        return await PostLoginAsync(serverAddress, port, "login", new { username, password }, "local", ct);
    }

    public async Task<AuthResult> LoginWithEmailAsync(string serverAddress, int port, string email, string password, CancellationToken ct = default)
    {
        return await PostLoginAsync(serverAddress, port, "login", new { email, password }, "email", ct);
    }

    public async Task<AuthResult> RegisterAsync(string serverAddress, int port, string username, string email, string password, CancellationToken ct = default)
    {
        try
        {
            var url = $"http://{serverAddress}:{port}/api/register";
            var payload = new { username, email, password };
            var resp = await _http.PostAsJsonAsync(url, payload, JsonOpts, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            var data = JsonSerializer.Deserialize<AuthResponse>(body, JsonOpts);

            if (data?.Success == true && !string.IsNullOrEmpty(data.UserId))
            {
                SetSession(data.UserId, data.Username, data.Email, data.Token, "local");
                return new AuthResult
                {
                    Success = true,
                    UserId = data.UserId,
                    Username = data.Username,
                    Email = data.Email,
                    Token = data.Token,
                    RegisterMethod = "local"
                };
            }

            return new AuthResult { Error = data?.Error ?? "Registration failed" };
        }
        catch (TaskCanceledException)
        {
            return new AuthResult { Error = "Connection timed out" };
        }
        catch (HttpRequestException ex)
        {
            return new AuthResult { Error = $"Cannot connect to server: {ex.Message}" };
        }
        catch
        {
            return new AuthResult { Error = "Unknown error" };
        }
    }

    public async Task<AuthResult> OfflineLoginAsync(string username, CancellationToken ct = default)
    {
        try
        {
            var path = System.IO.Path.Combine(StorageService.DataDir, "users", $"{username}.json");
            if (System.IO.File.Exists(path))
            {
                var json = await System.IO.File.ReadAllTextAsync(path, ct);
                var doc = JsonSerializer.Deserialize<JsonElement>(json);
                var uid = doc.TryGetProperty("id", out var id) ? id.GetString() ?? username : username;
                var em = doc.TryGetProperty("email", out var e) ? e.GetString() : null;

                SetSession(uid, username, em, null, "offline");
                return new AuthResult
                {
                    Success = true,
                    UserId = uid,
                    Username = username,
                    Email = em,
                    RegisterMethod = "offline"
                };
            }

            var userId = Guid.NewGuid().ToString("N");
            var dir = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                System.IO.Directory.CreateDirectory(dir);

            var userJson = JsonSerializer.Serialize(new { id = userId, username, created_at = DateTime.UtcNow });
            await System.IO.File.WriteAllTextAsync(path, userJson, ct);

            SetSession(userId, username, null, null, "offline");
            return new AuthResult
            {
                Success = true,
                UserId = userId,
                Username = username,
                RegisterMethod = "offline"
            };
        }
        catch (Exception ex)
        {
            return new AuthResult { Error = $"Offline login failed: {ex.Message}" };
        }
    }

    public Task LogoutAsync(CancellationToken ct = default)
    {
        _isLoggedIn = false;
        _userId = null;
        _username = null;
        _email = null;
        _token = null;
        _registerMethod = null;
        LoginStateChanged?.Invoke(this, false);
        return Task.CompletedTask;
    }

    public async Task<AuthResult> LoadSavedCredentialsAsync(CancellationToken ct = default)
    {
        try
        {
            var credPath = System.IO.Path.Combine(StorageService.DataDir, "credential.json");
            if (!System.IO.File.Exists(credPath))
                return new AuthResult { Error = "No saved credentials" };

            var json = await System.IO.File.ReadAllTextAsync(credPath, ct);
            var doc = JsonSerializer.Deserialize<JsonElement>(json);

            var username = doc.TryGetProperty("username", out var u) ? u.GetString() ?? "" : "";
            var email = doc.TryGetProperty("email", out var e) ? e.GetString() : null;
            var method = doc.TryGetProperty("method", out var m) ? m.GetString() ?? "local" : "local";

            if (string.IsNullOrEmpty(username))
                return new AuthResult { Error = "Invalid saved credentials" };

            return new AuthResult
            {
                Success = true,
                Username = username,
                Email = email,
                RegisterMethod = method
            };
        }
        catch
        {
            return new AuthResult { Error = "Failed to load credentials" };
        }
    }

    public async Task SaveCredentialsAsync(string username, string? email, string password, string method, CancellationToken ct = default)
    {
        var credPath = System.IO.Path.Combine(StorageService.DataDir, "credential.json");
        var dir = System.IO.Path.GetDirectoryName(credPath);
        if (!string.IsNullOrEmpty(dir))
            System.IO.Directory.CreateDirectory(dir);

        var data = new { username, email, password, method };
        var json = JsonSerializer.Serialize(data, JsonOpts);
        await System.IO.File.WriteAllTextAsync(credPath, json, ct);
    }

    public async Task ClearCredentialsAsync(CancellationToken ct = default)
    {
        var credPath = System.IO.Path.Combine(StorageService.DataDir, "credential.json");
        if (System.IO.File.Exists(credPath))
            await System.IO.File.DeleteAsync(credPath, ct);
    }

    private async Task<AuthResult> PostLoginAsync(string serverAddress, int port, string endpoint, object payload, string method, CancellationToken ct)
    {
        try
        {
            var url = $"http://{serverAddress}:{port}/api/{endpoint}";
            var resp = await _http.PostAsJsonAsync(url, payload, JsonOpts, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            var data = JsonSerializer.Deserialize<AuthResponse>(body, JsonOpts);

            if (data?.Success == true && !string.IsNullOrEmpty(data.UserId))
            {
                SetSession(data.UserId, data.Username, data.Email, data.Token, method);
                return new AuthResult
                {
                    Success = true,
                    UserId = data.UserId,
                    Username = data.Username,
                    Email = data.Email,
                    Token = data.Token,
                    RegisterMethod = method
                };
            }

            return new AuthResult { Error = data?.Error ?? "Login failed" };
        }
        catch (TaskCanceledException)
        {
            return new AuthResult { Error = "Connection timed out" };
        }
        catch (HttpRequestException ex)
        {
            return new AuthResult { Error = $"Cannot connect to server: {ex.Message}" };
        }
        catch
        {
            return new AuthResult { Error = "Unknown error" };
        }
    }

    private void SetSession(string userId, string? username, string? email, string? token, string method)
    {
        _isLoggedIn = true;
        _userId = userId;
        _username = username;
        _email = email;
        _token = token;
        _registerMethod = method;
        LoginStateChanged?.Invoke(this, true);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _http.Dispose();
        GC.SuppressFinalize(this);
    }
}
