using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AntiSwearingChatBox.Server.Controllers
{
    [ApiController]
    [Route("api/messages")]
    [Authorize] // All message operations require authentication
    public class MessageHistoryController : ControllerBase
    {
        private readonly IMessageHistoryService _messageHistoryService;

        public MessageHistoryController(IMessageHistoryService messageHistoryService)
        {
            _messageHistoryService = messageHistoryService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<MessageHistory>> GetAllMessages()
        {
            return Ok(_messageHistoryService.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<MessageHistory> GetMessageById(int id)
        {
            var message = _messageHistoryService.GetById(id);
            if (message == null)
            {
                return NotFound();
            }
            return Ok(message);
        }

        [HttpPost]
        public ActionResult<MessageHistory> CreateMessage(MessageHistory messageHistory)
        {
            var result = _messageHistoryService.Add(messageHistory);
            if (!result.success)
            {
                return BadRequest(result.message);
            }
            return CreatedAtAction(nameof(GetMessageById), new { id = messageHistory.MessageId }, messageHistory);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateMessage(int id, MessageHistory messageHistory)
        {
            if (id != messageHistory.MessageId)
            {
                return BadRequest("Message ID mismatch");
            }

            var result = _messageHistoryService.Update(messageHistory);
            if (!result.success)
            {
                return BadRequest(result.message);
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteMessage(int id)
        {
            var result = _messageHistoryService.Delete(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("thread")]
        public ActionResult<IEnumerable<MessageHistory>> GetMessagesByThread([FromQuery] int threadId)
        {
            return Ok(_messageHistoryService.GetByThreadId(threadId));
        }
    }
} 