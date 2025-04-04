using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AntiSwearingChatBox.AI;
using AntiSwearingChatBox.AI.Filter;

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
        public async Task<IActionResult> FilterProfanity([FromBody] FilterProfanityModel model)
        {
            try
            {
                string filteredText = await _profanityFilter.FilterProfanityAsync(model.Text);
                bool wasModified = filteredText != model.Text;
                
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