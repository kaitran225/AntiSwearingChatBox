using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Anti_Swearing_Chat_Box.AI
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeminiController : ControllerBase
    {
        private readonly GeminiService _geminiService;

        public GeminiController(GeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateText([FromBody] TextGenerationRequest request)
        {
            if (string.IsNullOrEmpty(request.Prompt))
            {
                return BadRequest("Prompt cannot be empty");
            }

            var result = await _geminiService.GenerateTextAsync(request.Prompt);
            return Ok(new { Text = result });
        }

        [HttpPost("moderate")]
        public async Task<IActionResult> ModerateChatMessage([FromBody] ModerationRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            var result = await _geminiService.ModerateChatMessageAsync(request.Message);
            return Ok(new { ModeratedText = result });
        }
    }

    public class TextGenerationRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }

    public class ModerationRequest
    {
        public string Message { get; set; } = string.Empty;
    }
} 