using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects.CalendarEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickStart.Presentation.Controllers
{
    [Route("api/event-calendar")]
    [ApiController]
    public class CalendarEventController : ControllerBase
    {
        private readonly IServiceManager _service;

        public CalendarEventController(IServiceManager service)
        {
            _service = service;
        }
        [HttpGet("{orderCode}")]
        public async Task<IActionResult> GetByCode(string orderCode)
        {
            var result = await _service.CalendarEventService.GetByOrderCodeAsync(orderCode, trackChanges: false);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody] CalendarEventDto dto)
        {
            await _service.CalendarEventService.CreateIfNotExistsAsync(dto);
            return Ok();
        }
    }
}
