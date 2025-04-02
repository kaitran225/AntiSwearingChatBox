using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AntiSwearingChatBox.Server.Controllers
{
    [ApiController]
    [Route("api/threads")]
    [Authorize] // Chat thread operations require authentication
    public class ChatThreadController : ControllerBase
    {
        private readonly IChatThreadService _chatThreadService;

        public ChatThreadController(IChatThreadService chatThreadService)
        {
            _chatThreadService = chatThreadService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ChatThread>> GetAllThreads()
        {
            return Ok(_chatThreadService.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<ChatThread> GetThreadById(int id)
        {
            var thread = _chatThreadService.GetById(id);
            if (thread == null)
            {
                return NotFound();
            }
            return Ok(thread);
        }

        [HttpPost]
        public ActionResult<ChatThread> CreateThread(ChatThread chatThread)
        {
            var result = _chatThreadService.Add(chatThread);
            if (!result.success)
            {
                return BadRequest(result.message);
            }
            return CreatedAtAction(nameof(GetThreadById), new { id = chatThread.ThreadId }, chatThread);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateThread(int id, ChatThread chatThread)
        {
            if (id != chatThread.ThreadId)
            {
                return BadRequest("Thread ID mismatch");
            }

            var result = _chatThreadService.Update(chatThread);
            if (!result.success)
            {
                return BadRequest(result.message);
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteThread(int id)
        {
            var result = _chatThreadService.Delete(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("user")]
        public ActionResult<IEnumerable<ChatThread>> GetThreadsByUser([FromQuery] int userId)
        {
            return Ok(_chatThreadService.GetUserThreads(userId));
        }
    }
} 