using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Server.Repo.Models;
using AntiSwearingChatBox.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;

namespace AntiSwearingChatBox.Server.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class ModeratorController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserWarningService _userWarningService;

        public ModeratorController(
            IUserService userService,
            IUserWarningService userWarningService)
        {
            _userService = userService;
            _userWarningService = userWarningService;
        }

        [HttpPost("users/{userId}/block")]
        public IActionResult BlockUser(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Can't block yourself
            if (userId == currentUserId)
                return BadRequest(new { message = "You cannot block yourself" });
            
            // Validate user exists
            var user = _userService.GetById(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });
            
            // Add a warning that represents a block
            var warning = new UserWarning
            {
                UserId = userId,
                ThreadId = 1, // Using default thread
                WarningMessage = $"Blocked by user {currentUserId}",
                CreatedAt = DateTime.UtcNow
            };
            
            _userWarningService.Add(warning);
            
            return Ok(new { message = "User blocked successfully" });
        }

        [HttpDelete("users/{userId}/block")]
        public IActionResult UnblockUser(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Validate user exists
            var user = _userService.GetById(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });
            
            // Add a "unblock" warning for record keeping
            var warning = new UserWarning
            {
                UserId = userId,
                ThreadId = 1, // Using default thread
                WarningMessage = $"Unblocked by user {currentUserId}",
                CreatedAt = DateTime.UtcNow
            };
            
            _userWarningService.Add(warning);
            
            return Ok(new { message = "User unblocked successfully" });
        }
    }
} 