using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AntiSwearingChatBox.AI.Interfaces;

namespace AntiSwearingChatBox.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IProfanityFilter _profanityFilter;

        public AIController(IProfanityFilter profanityFilter)
        {
            _profanityFilter = profanityFilter;
        }

        [HttpPost("filter-profanity")]
        public IActionResult FilterProfanity([FromBody] FilterProfanityModel model)
        {
            try
            {
                var (filteredText, wasModified) = _profanityFilter.FilterProfanity(model.Text);
                
                return Ok(new 
                {
                    Success = true,
                    OriginalText = model.Text,
                    FilteredText = filteredText,
                    WasModified = wasModified
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }
    }

    public class FilterProfanityModel
    {
        public string Text { get; set; } = string.Empty;
    }
} 