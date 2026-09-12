using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Talkift.Client.Models;

namespace Talkift.Client.Services
{
    public class AuthService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(10) };

        private string _token = string.Empty;
        private string _currentUserId = string.Empty;
        private string _currentUsername = string.Empty;
        private string _currentRegisterMethod = string.Empty;

        public bool IsAuthenticated => !string.IsNullOrEmpty(_token);
        public string Token => _token;
        public string CurrentUserId => _currentUserId;
        public string CurrentUsername => _currentUsername;
        public string CurrentRegisterMethod => _currentRegisterMethod;

        public async Task<AuthResponse> LoginAsync(string username, string password, string address, int port)
        {
            try
            {
                var url = $"http://{address}:{port}/api/login";
                var request = new AuthRequest { Username = username, Password = password };
                var response = await Client.PostAsJsonAsync(url, request);

                var body = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AuthResponse>(body, JsonOptions);

                if (result != null && result.Success)
                {
                    _token = result.Token ?? string.Empty;
                    _currentUserId = result.UUID ?? string.Empty;
                    _currentUsername = result.Username ?? username;
                    _currentRegisterMethod = result.RegisterMethod ?? "local";
                }

                return result ?? new AuthResponse { Success = false, Message = "No response from server" };
            }
            catch (HttpRequestException)
            {
                return new AuthResponse { Success = false, Code = 1005, Message = "Cannot connect to server" };
            }
            catch (TaskCanceledException)
            {
                return new AuthResponse { Success = false, Code = 1005, Message = "Connection timed out" };
            }
            catch (Exception ex)
            {
                return new AuthResponse { Success = false, Code = 1007, Message = ex.Message };
            }
        }

        public async Task<AuthResponse> LoginWithEmailAsync(string email, string password, string address, int port)
        {
            try
            {
                var url = $"http://{address}:{port}/api/login/email";
                var request = new AuthRequest { Email = email, Password = password };
                var response = await Client.PostAsJsonAsync(url, request);

                var body = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AuthResponse>(body, JsonOptions);

                if (result != null && result.Success)
                {
                    _token = result.Token ?? string.Empty;
                    _currentUserId = result.UUID ?? string.Empty;
                    _currentUsername = result.Username ?? string.Empty;
                    _currentRegisterMethod = result.RegisterMethod ?? "official";
                }

                return result ?? new AuthResponse { Success = false, Message = "No response from server" };
            }
            catch (HttpRequestException)
            {
                return new AuthResponse { Success = false, Code = 1005, Message = "Cannot connect to server" };
            }
            catch (TaskCanceledException)
            {
                return new AuthResponse { Success = false, Code = 1005, Message = "Connection timed out" };
            }
            catch (Exception ex)
            {
                return new AuthResponse { Success = false, Code = 1007, Message = ex.Message };
            }
        }

        public async Task<AuthResponse> OfflineLoginAsync(string username, string address, int port)
        {
            try
            {
                var url = $"http://{address}:{port}/api/login/offline";
                var request = new AuthRequest { Username = username, Password = "" };
                var response = await Client.PostAsJsonAsync(url, request);

                var body = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AuthResponse>(body, JsonOptions);

                if (result != null && result.Success)
                {
                    _token = result.Token ?? string.Empty;
                    _currentUserId = result.UUID ?? string.Empty;
                    _currentUsername = result.Username ?? username;
                    _currentRegisterMethod = result.RegisterMethod ?? "local";
                }

                return result ?? new AuthResponse { Success = false, Message = "No response from server" };
            }
            catch (HttpRequestException)
            {
                return new AuthResponse { Success = false, Code = 1005, Message = "Cannot connect to server" };
            }
            catch (TaskCanceledException)
            {
                return new AuthResponse { Success = false, Code = 1005, Message = "Connection timed out" };
            }
            catch (Exception ex)
            {
                return new AuthResponse { Success = false, Code = 1007, Message = ex.Message };
            }
        }

        public async Task<AuthResponse> RegisterAsync(string username, string password, RegisterMode mode, string address, int port, string? email = null)
        {
            try
            {
                var method = mode switch
                {
                    RegisterMode.Official => "official",
                    RegisterMode.Local => "local",
                    _ => "local"
                };

                var url = $"http://{address}:{port}/api/register";

                var request = new AuthRequest
                {
                    Username = username,
                    Password = password,
                    Email = email,
                    RegisterMethod = method
                };

                var response = await Client.PostAsJsonAsync(url, request);
                var body = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AuthResponse>(body, JsonOptions);

                if (result != null && result.Success)
                {
                    _token = result.Token ?? string.Empty;
                    _currentUserId = result.UUID ?? string.Empty;
                    _currentUsername = result.Username ?? username;
                    _currentRegisterMethod = method;
                }

                return result ?? new AuthResponse { Success = false, Message = "No response from server" };
            }
            catch (HttpRequestException)
            {
                return new AuthResponse { Success = false, Code = 1005, Message = "Cannot connect to server" };
            }
            catch (TaskCanceledException)
            {
                return new AuthResponse { Success = false, Code = 1005, Message = "Connection timed out" };
            }
            catch (Exception ex)
            {
                return new AuthResponse { Success = false, Code = 1007, Message = ex.Message };
            }
        }

        public void SetCredentials(string token, string userId, string username, string registerMethod)
        {
            _token = token;
            _currentUserId = userId;
            _currentUsername = username;
            _currentRegisterMethod = registerMethod;
        }

        public void Logout()
        {
            _token = string.Empty;
            _currentUserId = string.Empty;
            _currentUsername = string.Empty;
            _currentRegisterMethod = string.Empty;
        }
    }
}
