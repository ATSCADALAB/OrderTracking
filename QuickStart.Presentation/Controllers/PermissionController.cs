using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickStart.Presentation.ActionFilters;
using Service.Contracts;
using Shared.DataTransferObjects.Permission;
using System.Security.Claims;

namespace QuickStart.Presentation.Controllers
{
    [Route("api/permissions")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class PermissionController : ControllerBase
    {
        private readonly IServiceManager _service;
        public PermissionController(IServiceManager service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetPermissions()
        {
            var Permissions = await _service.PermissionService.GetAllPermissionsAsync(trackChanges: false);

            return Ok(Permissions);
        }

        [HttpGet("{id:guid}", Name = "PermissionById")]
        public async Task<IActionResult> GetPermission(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //if (!await _service.AuthorizationService.HasPermission(userId, "Permissions", "Edit"))
            //{
            //    return Forbid(); // 403 Forbidden
            //}
            var Permission = await _service.PermissionService.GetPermissionAsync(id, trackChanges: false);
            return Ok(Permission);
        }


        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreatePermission([FromBody] PermissionForCreationDto Permission)
        {
            var createdPermission = await _service.PermissionService.CreatePermissionAsync(Permission);

            return CreatedAtRoute("PermissionById", new { id = createdPermission.Id }, createdPermission);
        }


        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePermission(Guid id)
        {
            await _service.PermissionService.DeletePermissionAsync(id, trackChanges: false);

            return NoContent();
        }

        [HttpPut("{id:guid}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdatePermission(Guid id, [FromBody] PermissionForUpdateDto Permission)
        {
            await _service.PermissionService.UpdatePermissionAsync(id, Permission, trackChanges: true);

            return NoContent();
        }
    }
}
