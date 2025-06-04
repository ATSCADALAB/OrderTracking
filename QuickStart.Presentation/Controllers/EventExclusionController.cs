using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickStart.Presentation.ActionFilters;
using Service.Contracts;
using Shared.DataTransferObjects.EventExclusion;

namespace QuickStart.Presentation.Controllers
{
    [Route("api/event-exclusion")]
    [ApiController]
    [Authorize]
    public class EventExclusionController : ControllerBase
    {
        private readonly IServiceManager _service;

        public EventExclusionController(IServiceManager service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllKeywords()
        {
            var keywords = await _service.EventExclusionKeywordService.GetAllKeywordsAsync();
            return Ok(keywords);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveKeywords()
        {
            var keywords = await _service.EventExclusionKeywordService.GetActiveKeywordsAsync();
            return Ok(keywords);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetKeyword(int id)
        {
            var keyword = await _service.EventExclusionKeywordService.GetKeywordByIdAsync(id);
            if (keyword == null)
                return NotFound();
            return Ok(keyword);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateKeyword([FromBody] EventExclusionKeywordForCreationDto keywordDto)
        {
            var created = await _service.EventExclusionKeywordService.CreateKeywordAsync(keywordDto);
            return CreatedAtAction(nameof(GetKeyword), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateKeyword(int id, [FromBody] EventExclusionKeywordForUpdateDto keywordDto)
        {
            await _service.EventExclusionKeywordService.UpdateKeywordAsync(id, keywordDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKeyword(int id)
        {
            await _service.EventExclusionKeywordService.DeleteKeywordAsync(id);
            return NoContent();
        }

        [HttpPost("check")]
        public async Task<IActionResult> CheckEventTitle([FromBody] string eventTitle)
        {
            var shouldExclude = await _service.EventExclusionKeywordService.ShouldExcludeEventAsync(eventTitle);
            return Ok(new { EventTitle = eventTitle, ShouldExclude = shouldExclude });
        }
    }
}