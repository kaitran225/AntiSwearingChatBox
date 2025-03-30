using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service;
using AntiSwearingChatBox.Service.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AntiSwearingChatBox.Server.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly JwtSettings _jwtSettings;

        public AuthService(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;

            // Load JWT settings from configuration
            _jwtSettings = new JwtSettings
            {
                SecretKey = _configuration["JwtSettings:SecretKey"] ?? "your-super-secret-key-with-minimum-32-characters",
                Issuer = _configuration["JwtSettings:Issuer"] ?? "AntiSwearingChatBox",
                Audience = _configuration["JwtSettings:Audience"] ?? "AntiSwearingChatBox",
                ExpirationInMinutes = int.TryParse(_configuration["JwtSettings:ExpirationInMinutes"], out int exp) ? exp : 60,
                RefreshTokenExpirationInDays = int.TryParse(_configuration["JwtSettings:RefreshTokenExpirationInDays"], out int rtExp) ? rtExp : 7
            };
        }

        public (bool success, string message, string token, User? user) Login(string username, string password)
        {
            try
            {
                // Authenticate user
                var (success, message, user) = _userService.Authenticate(username, password);
                if (!success || user == null)
                {
                    return (false, message, string.Empty, null);
                }

                // Generate JWT token
                var token = GenerateJwtToken(user);

                return (true, "Login successful", token, user);
            }
            catch (Exception ex)
            {
                return (false, $"Login error: {ex.Message}", string.Empty, null);
            }
        }

        public (bool success, string message) Register(string username, string email, string password)
        {
            try
            {
                // Create new user
                var user = new User
                {
                    Username = username,
                    Email = email,
                    IsActive = true,
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    Role = "User",
                    TrustScore = 1.0m
                };

                // Register user
                return _userService.Register(user, password);
            }
            catch (Exception ex)
            {
                return (false, $"Registration error: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> VerifyAccountAsync(string token)
        {
            // In a real implementation, we would do an asynchronous operation here
            await Task.Yield(); // Add a minimal await operation to suppress warning
            return (true, "Account verified successfully");
        }

        public Task<(bool success, string message)> RequestPasswordResetAsync(string email)
        {
            // This is a placeholder for password reset
            // In a real implementation, you would generate a reset token and send an email to the user
            return Task.FromResult<(bool success, string message)>((true, "Password reset email sent"));
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<(bool success, string message, string token, string refreshToken)> RegisterAsync(User user, string password)
        {
            var (success, message) = Register(user.Username, user.Email, password);
            await Task.Yield(); // Add minimal await operation to suppress warning
            return (success, message, "dummy-token", "dummy-refresh-token");
        }

        public async Task<(bool success, string message, string token, string refreshToken)> LoginAsync(string username, string password)
        {
            var (success, message, token, _) = Login(username, password);
            await Task.Yield(); // Add minimal await operation to suppress warning
            return (success, message, token, "dummy-refresh-token");
        }

        public async Task<(bool success, string message, string token, string refreshToken)> RefreshTokenAsync(string refreshToken)
        {
            await Task.Yield(); // Add minimal await operation to suppress warning
            return (true, "Token refreshed", "new-dummy-token", "new-dummy-refresh-token");
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            await Task.Yield(); // Add minimal await operation to suppress warning
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            await Task.Yield(); // Add minimal await operation to suppress warning
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            await Task.Yield(); // Add minimal await operation to suppress warning
            return true;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            await Task.Yield(); // Add minimal await operation to suppress warning
            return true;
        }
    }
}