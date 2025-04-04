using AntiSwearingChatBox.Server.Repo.Models;
using AntiSwearingChatBox.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AntiSwearingChatBox.Server.Controllers
{
    [ApiController]
    [Route("api/warnings")]
    [Authorize] // Warning operations require authentication
    public class UserWarningController : ControllerBase
    {
        private readonly IUserWarningService _userWarningService;

        public UserWarningController(IUserWarningService userWarningService)
        {
            _userWarningService = userWarningService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<UserWarning>> GetAllWarnings()
        {
            return Ok(_userWarningService.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<UserWarning> GetWarningById(int id)
        {
            var warning = _userWarningService.GetById(id);
            if (warning == null)
            {
                return NotFound();
            }
            return Ok(warning);
        }

        [HttpPost]
        public ActionResult<UserWarning> CreateWarning(UserWarning userWarning)
        {
            var result = _userWarningService.Add(userWarning);
            if (!result.success)
            {
                return BadRequest(result.message);
            }
            return CreatedAtAction(nameof(GetWarningById), new { id = userWarning.WarningId }, userWarning);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateWarning(int id, UserWarning userWarning)
        {
            if (id != userWarning.WarningId)
            {
                return BadRequest("Warning ID mismatch");
            }

            var result = _userWarningService.Update(userWarning);
            if (!result.success)
            {
                return BadRequest(result.message);
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteWarning(int id)
        {
            var result = _userWarningService.Delete(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("user")]
        public ActionResult<IEnumerable<UserWarning>> GetWarningsByUser([FromQuery] int userId)
        {
            return Ok(_userWarningService.Search(userId.ToString()));
        }
    }
} 