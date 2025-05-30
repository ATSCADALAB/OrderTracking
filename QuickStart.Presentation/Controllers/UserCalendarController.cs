using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickStart.Presentation.ActionFilters;
using Service.Contracts;
using Shared.DataTransferObjects.UserCalendar;
using System.Security.Claims;

namespace QuickStart.Presentation.Controllers
{
    [Route("api/usercalendars")]
    [ApiController]
    //[Authorize]
    public class UserCalendarController : ControllerBase
    {
        private readonly IServiceManager _service;

        public UserCalendarController(IServiceManager service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserCalendars()
        {
            var calendars = await _service.UserCalendarService.GetAllUserCalendarsAsync(trackChanges: false);
            return Ok(calendars);
        }

        [HttpGet("{id:guid}", Name = "UserCalendarById")]
        public async Task<IActionResult> GetUserCalendar(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!await _service.AuthorizationService.HasPermission(userId, "UserCalendars", "View"))
            {
                return Forbid();
            }

            var calendar = await _service.UserCalendarService.GetUserCalendarAsync(id, trackChanges: false);
            return Ok(calendar);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateUserCalendar([FromBody] UserCalendarForCreationDto dto)
        {
            var created = await _service.UserCalendarService.CreateUserCalendarAsync(dto);
            return CreatedAtRoute("UserCalendarById", new { id = created.Id }, created);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUserCalendar(Guid id)
        {
            await _service.UserCalendarService.DeleteUserCalendarAsync(id, trackChanges: false);
            return NoContent();
        }
    }
}
