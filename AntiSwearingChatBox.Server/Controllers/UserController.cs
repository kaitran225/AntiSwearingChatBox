using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AntiSwearingChatBox.Server.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize] // Default to authorized
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<User>> GetAllUsers()
        {
            return Ok(_userService.GetAll());
        }

        [HttpGet("{id}")]
        [Authorize]
        public ActionResult<User> GetUserById(int id)
        {
            var user = _userService.GetById(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpGet("find")]
        [Authorize]
        public ActionResult<User> GetUserByUsername([FromQuery] string username)
        {
            var user = _userService.GetByUsername(username);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost("register")]
        [AllowAnonymous] // Registration should be open
        public ActionResult<User> RegisterUser(RegisterRequestModel request)
        {
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Gender = request.Gender
            };

            var result = _userService.Register(user, request.Password);
            if (!result.success)
            {
                return BadRequest(result.message);
            }
            return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, user);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, User user)
        {
            if (id != user.UserId)
            {
                return BadRequest("User ID mismatch");
            }

            var result = _userService.Update(user);
            if (!result.success)
            {
                return BadRequest(result.message);
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var result = _userService.Delete(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }

    public class RegisterRequestModel
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Gender { get; set; }
    }
} 