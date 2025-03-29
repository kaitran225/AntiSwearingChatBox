using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AntiSwearingChatBox.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Gender = request.Gender
            };

            var (success, message, token, refreshToken) = await _authService.RegisterAsync(user, request.Password);
            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var (success, message, token, refreshToken) = await _authService.LoginAsync(request.Username, request.Password);
            if (!success)
                return BadRequest(new { message });

            return Ok(new { message, token, refreshToken });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var (success, message, token, refreshToken) = await _authService.RefreshTokenAsync(request.RefreshToken);
            if (!success)
                return BadRequest(new { message });

            return Ok(new { message, token, refreshToken });
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            var success = await _authService.RevokeTokenAsync(request.RefreshToken);
            if (!success)
                return BadRequest(new { message = "Invalid refresh token" });

            return Ok(new { message = "Token revoked successfully" });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var success = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            
            if (!success)
                return BadRequest(new { message = "Failed to change password" });

            return Ok(new { message = "Password changed successfully" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var success = await _authService.ResetPasswordAsync(request.Email);
            if (!success)
                return BadRequest(new { message = "Failed to reset password" });

            return Ok(new { message = "Password reset email sent" });
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var success = await _authService.VerifyEmailAsync(token);
            if (!success)
                return BadRequest(new { message = "Invalid verification token" });

            return Ok(new { message = "Email verified successfully" });
        }
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Gender { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = null!;
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; } = null!;
    }
} 