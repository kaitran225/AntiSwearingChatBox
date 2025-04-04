using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using AntiSwearingChatBox.Server.AI;

namespace AntiSwearingChatBox.AI
{
    /// <summary>
    /// API controller for accessing Gemini AI capabilities directly
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GeminiController : ControllerBase
    {
        private readonly GeminiService _geminiService;

        public GeminiController(GeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        /// <summary>
        /// Generates text based on a prompt
        /// </summary>
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

        /// <summary>
        /// Moderates a chat message, replacing profanity with alternatives
        /// </summary>
        [HttpPost("moderate")]
        public async Task<IActionResult> ModerateChatMessage([FromBody] ModerationRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            var result = await _geminiService.ModerateChatMessageAsync(request.Message);
            return Content(result, "application/json");
        }
        
        /// <summary>
        /// Detects if a message contains profanity
        /// </summary>
        [HttpPost("detect-profanity")]
        public async Task<IActionResult> DetectProfanity([FromBody] ModerationRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            // If we're requesting verbose details, we need to include all processing steps
            if (request.IncludeDetails)
            {
                var detailedResult = await _geminiService.DetectProfanityWithDetailsAsync(request.Message);
                return Content(detailedResult, "application/json");
            }
            else
            {
                // Regular detection without details
                var result = await _geminiService.DetectProfanityAsync(request.Message);
                return Content(result, "application/json");
            }
        }
        
        /// <summary>
        /// Performs context-aware filtering considering conversation history
        /// </summary>
        [HttpPost("context-filter")]
        public async Task<IActionResult> ContextAwareFiltering([FromBody] ContextFilterRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            var result = await _geminiService.PerformContextAwareFilteringAsync(request.Message, request.ConversationContext);
            return Content(result, "application/json");
        }
        
        /// <summary>
        /// Analyzes the sentiment and toxicity of a message
        /// </summary>
        [HttpPost("analyze-sentiment")]
        public async Task<IActionResult> AnalyzeSentiment([FromBody] ModerationRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            var result = await _geminiService.AnalyzeSentimentAsync(request.Message);
            return Content(result, "application/json");
        }
        
        /// <summary>
        /// Generates a de-escalation response for a harmful message
        /// </summary>
        [HttpPost("de-escalate")]
        public async Task<IActionResult> GenerateDeescalationResponse([FromBody] ModerationRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            var result = await _geminiService.GenerateDeescalationResponseAsync(request.Message);
            return Content(result, "application/json");
        }
        
        /// <summary>
        /// Reviews a history of messages for patterns of inappropriate behavior
        /// </summary>
        [HttpPost("review-message-history")]
        public async Task<IActionResult> ReviewMessageHistory([FromBody] MessageHistoryRequest request)
        {
            if (request.Messages == null || request.Messages.Count == 0)
            {
                return BadRequest("Message history cannot be empty");
            }

            var result = await _geminiService.ReviewMessageHistoryAsync(request.Messages);
            return Content(result, "application/json");
        }
        
        /// <summary>
        /// Suggests an alternative for an inappropriate message
        /// </summary>
        [HttpPost("suggest-alternative")]
        public async Task<IActionResult> SuggestAlternativeMessage([FromBody] ModerationRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            var result = await _geminiService.SuggestAlternativeMessageAsync(request.Message);
            return Content(result, "application/json");
        }
        
        /// <summary>
        /// Moderates a message in a specific language
        /// </summary>
        [HttpPost("moderate-multi-language")]
        public async Task<IActionResult> ModerateMultiLanguageMessage([FromBody] MultiLanguageRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }
            
            // If language is empty or "auto", let the AI detect the language
            string language = string.IsNullOrEmpty(request.Language) || request.Language.ToLower() == "auto" 
                ? "auto-detect" 
                : request.Language;

            var result = await _geminiService.ModerateMultiLanguageMessageAsync(request.Message, language);
            return Content(result, "application/json");
        }
        
        /// <summary>
        /// Analyzes a user's reputation based on message history and prior warnings
        /// </summary>
        [HttpPost("analyze-user-reputation")]
        public async Task<IActionResult> AnalyzeUserReputation([FromBody] UserReputationRequest request)
        {
            if (request.Messages == null || request.Messages.Count == 0)
            {
                return BadRequest("Message history cannot be empty");
            }

            var result = await _geminiService.AnalyzeUserReputationAsync(request.Messages, request.PriorWarnings);
            return Content(result, "application/json");
        }
    }

    public class TextGenerationRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }

    public class ModerationRequest
    {
        public string Message { get; set; } = string.Empty;
        public bool IncludeDetails { get; set; } = false;
    }
    
    public class ContextFilterRequest
    {
        public string Message { get; set; } = string.Empty;
        public string ConversationContext { get; set; } = string.Empty;
    }
    
    public class MessageHistoryRequest
    {
        public List<string> Messages { get; set; } = new List<string>();
    }
    
    public class MultiLanguageRequest
    {
        public string Message { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
    }
    
    public class UserReputationRequest
    {
        public List<string> Messages { get; set; } = new List<string>();
        public int PriorWarnings { get; set; } = 0;
    }
} 