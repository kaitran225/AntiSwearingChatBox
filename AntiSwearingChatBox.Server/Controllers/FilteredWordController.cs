using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AntiSwearingChatBox.Server.Controllers
{
    [ApiController]
    [Route("api/filteredwords")]
    [Authorize] // Filtered word operations should be protected
    public class FilteredWordController : ControllerBase
    {
        private readonly IFilteredWordService _filteredWordService;

        public FilteredWordController(IFilteredWordService filteredWordService)
        {
            _filteredWordService = filteredWordService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<FilteredWord>> GetAllFilteredWords()
        {
            return Ok(_filteredWordService.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<FilteredWord> GetFilteredWordById(int id)
        {
            var filteredWord = _filteredWordService.GetById(id);
            if (filteredWord == null)
            {
                return NotFound();
            }
            return Ok(filteredWord);
        }

        [HttpPost]
        public ActionResult<FilteredWord> CreateFilteredWord(FilteredWord filteredWord)
        {
            var result = _filteredWordService.Add(filteredWord);
            if (!result.success)
            {
                return BadRequest(result.message);
            }
            return CreatedAtAction(nameof(GetFilteredWordById), new { id = filteredWord.WordId }, filteredWord);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateFilteredWord(int id, FilteredWord filteredWord)
        {
            if (id != filteredWord.WordId)
            {
                return BadRequest("Filtered Word ID mismatch");
            }

            var result = _filteredWordService.Update(filteredWord);
            if (!result.success)
            {
                return BadRequest(result.message);
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteFilteredWord(int id)
        {
            var result = _filteredWordService.Delete(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("search")]
        public ActionResult<IEnumerable<FilteredWord>> SearchFilteredWords([FromQuery] string term)
        {
            return Ok(_filteredWordService.Search(term));
        }
    }
} 