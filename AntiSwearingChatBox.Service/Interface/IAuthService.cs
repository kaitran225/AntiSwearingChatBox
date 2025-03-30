using AntiSwearingChatBox.Repository.Models;
using System.Threading.Tasks;

namespace AntiSwearingChatBox.Service.Interface
{
    public interface IAuthService
    {
        (bool success, string message, string token, User? user) Login(string username, string password);
        (bool success, string message) Register(string username, string email, string password);
        Task<(bool success, string message)> VerifyAccountAsync(string token);
        Task<(bool success, string message)> RequestPasswordResetAsync(string email);

        // Add these methods to match controller expectations
        Task<(bool success, string message, string token, string refreshToken)> RegisterAsync(User user, string password);
        Task<(bool success, string message, string token, string refreshToken)> LoginAsync(string username, string password);
        Task<(bool success, string message, string token, string refreshToken)> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string refreshToken);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email);
        Task<bool> VerifyEmailAsync(string token);
    }
}