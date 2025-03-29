using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AntiSwearingChatBox.Repository.IRepositories;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.IServices;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AntiSwearingChatBox.Service.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSettings _jwtSettings;

        public AuthService(IUnitOfWork unitOfWork, IOptions<JwtSettings> jwtSettings)
        {
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<(bool success, string message, string? token, string? refreshToken)> LoginAsync(string username, string password)
        {
            var user = _unitOfWork.User.Find(u => u.Username == username).FirstOrDefault();
            if (user == null)
                return (false, "User not found", null, null);

            if (!user.IsActive)
                return (false, "Account is deactivated", null, null);

            if (!VerifyPasswordHash(password, user.PasswordHash))
                return (false, "Invalid password", null, null);

            if (!user.IsVerified)
                return (false, "Please verify your email first", null, null);

            user.LastLoginAt = DateTime.UtcNow;
            _unitOfWork.User.Update(user);
            await _unitOfWork.CompleteAsync();

            var (token, refreshToken) = GenerateTokens(user);
            return (true, "Login successful", token, refreshToken);
        }

        public async Task<(bool success, string message, string? token, string? refreshToken)> RegisterAsync(User user, string password)
        {
            if (_unitOfWork.User.Find(u => u.Username == user.Username).Any())
                return (false, "Username already exists", null, null);

            if (_unitOfWork.User.Find(u => u.Email == user.Email).Any())
                return (false, "Email already exists", null, null);

            user.PasswordHash = HashPassword(password);
            user.VerificationToken = GenerateVerificationToken();
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;
            user.IsVerified = false;
            user.Role = "User";
            user.TrustScore = 1.0m;

            _unitOfWork.User.Add(user);
            await _unitOfWork.CompleteAsync();

            // TODO: Send verification email

            return (true, "Registration successful. Please verify your email.", null, null);
        }

        public async Task<(bool success, string message, string? token, string? refreshToken)> RefreshTokenAsync(string refreshToken)
        {
            var user = _unitOfWork.User.Find(u => u.ResetToken == refreshToken && 
                                                u.TokenExpiration > DateTime.UtcNow).FirstOrDefault();

            if (user == null)
                return (false, "Invalid refresh token", null, null);

            var (newToken, newRefreshToken) = GenerateTokens(user);
            return (true, "Token refreshed successfully", newToken, newRefreshToken);
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            var user = _unitOfWork.User.Find(u => u.ResetToken == refreshToken).FirstOrDefault();
            if (user == null)
                return false;

            user.ResetToken = null;
            user.TokenExpiration = null;
            _unitOfWork.User.Update(user);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = _unitOfWork.User.GetById(userId);
            if (user == null)
                return false;

            if (!VerifyPasswordHash(currentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = HashPassword(newPassword);
            _unitOfWork.User.Update(user);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            var user = _unitOfWork.User.Find(u => u.Email == email).FirstOrDefault();
            if (user == null)
                return false;

            user.ResetToken = GenerateVerificationToken();
            user.TokenExpiration = DateTime.UtcNow.AddHours(24);
            _unitOfWork.User.Update(user);
            await _unitOfWork.CompleteAsync();

            // TODO: Send password reset email
            return true;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var user = _unitOfWork.User.Find(u => u.VerificationToken == token).FirstOrDefault();
            if (user == null)
                return false;

            user.IsVerified = true;
            user.VerificationToken = null;
            _unitOfWork.User.Update(user);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        private (string token, string refreshToken) GenerateTokens(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = GenerateVerificationToken();

            user.ResetToken = refreshToken;
            user.TokenExpiration = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);
            _unitOfWork.User.Update(user);
            _unitOfWork.CompleteAsync().Wait();

            return (tokenHandler.WriteToken(token), refreshToken);
        }

        private string HashPassword(string password)
        {
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            var combined = new byte[salt.Length + hash.Length];
            Array.Copy(salt, 0, combined, 0, salt.Length);
            Array.Copy(hash, 0, combined, salt.Length, hash.Length);
            return Convert.ToBase64String(combined);
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            var combined = Convert.FromBase64String(storedHash);
            var salt = new byte[64];
            var hash = new byte[64];
            Array.Copy(combined, 0, salt, 0, salt.Length);
            Array.Copy(combined, salt.Length, hash, 0, hash.Length);

            using var hmac = new HMACSHA512(salt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(hash);
        }

        private string GenerateVerificationToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
    }
} 