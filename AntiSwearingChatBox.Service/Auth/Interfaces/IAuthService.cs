using AntiSwearingChatBox.Repository.Models;

namespace AntiSwearingChatBox.Service.IServices
{
    public interface IAuthService
    {
        Task<(bool success, string message, string? token, string? refreshToken)> LoginAsync(string username, string password);
        Task<(bool success, string message, string? token, string? refreshToken)> RegisterAsync(User user, string password);
        Task<(bool success, string message, string? token, string? refreshToken)> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string refreshToken);
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email);
        Task<bool> VerifyEmailAsync(string token);
    }
} 