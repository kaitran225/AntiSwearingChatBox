using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

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

        [HttpPut("{threadId}/swearing-score")]
        public IActionResult UpdateSwearingScore(int threadId, [FromBody] UpdateScoreModel model)
        {
            try
            {
                // Get the thread
                var thread = _chatThreadService.GetById(threadId);
                if (thread == null)
                {
                    return NotFound(new { Success = false, Message = "Thread not found" });
                }
                
                // Update the swearing score
                thread.SwearingScore = model.Score;
                
                // If the score is over the threshold, close the thread
                if (model.Score > 5)
                {
                    thread.IsClosed = true;
                }
                
                // Save the changes
                var result = _chatThreadService.Update(thread);
                if (!result.success)
                {
                    return StatusCode(500, new { Success = false, Message = result.message });
                }
                
                return Ok(new { Success = true, Message = "Swearing score updated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("{threadId}/close")]
        public IActionResult CloseThread(int threadId)
        {
            try
            {
                // Get the thread
                var thread = _chatThreadService.GetById(threadId);
                if (thread == null)
                {
                    return NotFound(new { Success = false, Message = "Thread not found" });
                }
                
                // Close the thread
                thread.IsClosed = true;
                
                // Save the changes
                var result = _chatThreadService.Update(thread);
                if (!result.success)
                {
                    return StatusCode(500, new { Success = false, Message = result.message });
                }
                
                // Note: SignalR notifications can be implemented in the future if needed
                
                return Ok(new { Success = true, Message = "Thread closed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        public class UpdateScoreModel
        {
            public int Score { get; set; }
        }
    }
} 