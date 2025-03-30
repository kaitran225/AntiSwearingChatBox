using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AntiSwearingChatBox.Server.Controllers
{
    [ApiController]
    [Route("api/participants")]
    [AllowAnonymous] // Allow anonymous access for testing
    public class ThreadParticipantController : ControllerBase
    {
        private readonly IThreadParticipantService _threadParticipantService;

        public ThreadParticipantController(IThreadParticipantService threadParticipantService)
        {
            _threadParticipantService = threadParticipantService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ThreadParticipant>> GetAllParticipants()
        {
            return Ok(_threadParticipantService.GetAll());
        }

        // For thread participants, use composite key approach with query parameters instead
        [HttpGet("find")]
        public ActionResult<ThreadParticipant> GetParticipantById([FromQuery] int threadId, [FromQuery] int userId)
        {
            // Create composite key
            var compositeKey = $"{threadId}_{userId}";
            var participant = _threadParticipantService.GetById(compositeKey);
            if (participant == null)
            {
                return NotFound();
            }
            return Ok(participant);
        }

        [HttpPost]
        public ActionResult<ThreadParticipant> AddParticipant(ThreadParticipant threadParticipant)
        {
            var result = _threadParticipantService.Add(threadParticipant);
            if (!result.success)
            {
                return BadRequest(result.message);
            }
            // For composite key entities, use query parameters
            return CreatedAtAction(
                nameof(GetParticipantById), 
                new { threadId = threadParticipant.ThreadId, userId = threadParticipant.UserId }, 
                threadParticipant
            );
        }

        // For update, use route with two parameters
        [HttpPut("thread/{threadId}/user/{userId}")]
        public IActionResult UpdateParticipant(int threadId, int userId, ThreadParticipant threadParticipant)
        {
            if (threadId != threadParticipant.ThreadId || userId != threadParticipant.UserId)
            {
                return BadRequest("Participant ID mismatch");
            }

            var compositeKey = $"{threadId}_{userId}";
            var result = _threadParticipantService.Update(threadParticipant);
            if (!result.success)
            {
                return BadRequest(result.message);
            }
            return NoContent();
        }

        // For delete, use route with two parameters
        [HttpDelete("thread/{threadId}/user/{userId}")]
        public IActionResult DeleteParticipant(int threadId, int userId)
        {
            var compositeKey = $"{threadId}_{userId}";
            var result = _threadParticipantService.Delete(compositeKey);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("thread")]
        public ActionResult<IEnumerable<ThreadParticipant>> GetParticipantsByThread([FromQuery] int threadId)
        {
            return Ok(_threadParticipantService.GetByThreadId(threadId));
        }

        [HttpGet("user")]
        public ActionResult<IEnumerable<ThreadParticipant>> GetParticipantsByUser([FromQuery] int userId)
        {
            return Ok(_threadParticipantService.GetByUserId(userId));
        }
    }
} 