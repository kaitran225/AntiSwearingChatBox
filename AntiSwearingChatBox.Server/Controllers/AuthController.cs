using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AntiSwearingChatBox.Service.Interface;
using AntiSwearingChatBox.Repository.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AntiSwearingChatBox.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                var (success, message, token, _) = await _authService.LoginAsync(model.Username, model.Password);
                
                if (success)
                {
                    // Get full user details
                    var user = _userService.GetByUsername(model.Username);
                    
                    return Ok(new 
                    {
                        Success = true,
                        Message = "Login successful",
                        Token = token,
                        User = user
                    });
                }
                
                return BadRequest(new { Success = false, Message = message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    IsActive = true,
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    Role = "User"
                };

                var (success, message, _, _) = await _authService.RegisterAsync(user, model.Password);

                if (success)
                {
                    return Ok(new { Success = true, Message = "Registration successful" });
                }

                return BadRequest(new { Success = false, Message = message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            try
            {
                var users = _userService.GetAll();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("users/{id}")]
        public IActionResult GetUserById(int id)
        {
            try
            {
                var user = _userService.GetById(id);
                
                if (user == null)
                {
                    return NotFound(new { Success = false, Message = "User not found" });
                }
                
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
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

    public class LoginModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterModel
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
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