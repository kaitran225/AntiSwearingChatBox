using System.Net.Http.Json;
using System.Text.Json;
using AntiSwearingChatBox.Service.IServices;

namespace AntiSwearingChatBox.App.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _tokenKey = "auth_token";
        private readonly string _refreshTokenKey = "refresh_token";

        public AuthService(string baseUrl)
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
        }

        public async Task<(bool success, string message, string? token, string? refreshToken)> LoginAsync(string username, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", new
                {
                    username,
                    password
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result?.Token != null)
                    {
                        await SaveTokensAsync(result.Token, result.RefreshToken);
                        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);
                    }
                    return (true, result?.Message ?? "Login successful", result?.Token, result?.RefreshToken);
                }

                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return (false, error?.Message ?? "Login failed", null, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null, null);
            }
        }

        public async Task<(bool success, string message)> RegisterAsync(string username, string email, string password, string? gender = null)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", new
                {
                    username,
                    email,
                    password,
                    gender
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
                    return (true, result?.Message ?? "Registration successful");
                }

                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return (false, error?.Message ?? "Registration failed");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<bool> RefreshTokenAsync()
        {
            var refreshToken = await GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken))
                return false;

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/refresh-token", new
                {
                    refreshToken
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result?.Token != null)
                    {
                        await SaveTokensAsync(result.Token, result.RefreshToken);
                        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);
                        return true;
                    }
                }
            }
            catch
            {
                // Log error if needed
            }

            return false;
        }

        public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/change-password", new
                {
                    currentPassword,
                    newPassword
                });

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", new
                {
                    email
                });

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/auth/verify-email?token={token}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("api/auth/validate");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            var refreshToken = await GetRefreshTokenAsync();
            if (!string.IsNullOrEmpty(refreshToken))
            {
                try
                {
                    await _httpClient.PostAsJsonAsync("api/auth/revoke-token", new { refreshToken });
                }
                catch
                {
                    // Log error if needed
                }
            }

            await ClearTokensAsync();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        private async Task SaveTokensAsync(string token, string refreshToken)
        {
            await SecureStorage.Default.SetAsync(_tokenKey, token);
            await SecureStorage.Default.SetAsync(_refreshTokenKey, refreshToken);
        }

        private async Task<string?> GetTokenAsync()
        {
            return await SecureStorage.Default.GetAsync(_tokenKey);
        }

        private async Task<string?> GetRefreshTokenAsync()
        {
            return await SecureStorage.Default.GetAsync(_refreshTokenKey);
        }

        private async Task ClearTokensAsync()
        {
            SecureStorage.Default.Remove(_tokenKey);
            SecureStorage.Default.Remove(_refreshTokenKey);
        }
    }

    public class LoginResponse
    {
        public string Message { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }

    public class RegisterResponse
    {
        public string Message { get; set; } = null!;
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = null!;
    }
} 