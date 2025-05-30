using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickStart.Presentation.ActionFilters;
using QuickStart.Shared.DataTransferObjects.RolePermission;
using Service.Contracts;
using Shared.DataTransferObjects.RolePermission;
using System.Security.Claims;

namespace QuickStart.Presentation.Controllers
{
    [Route("api/rolepermissions")]
    [ApiController]
    public class RolePermissionController : ControllerBase
    {
        private readonly IServiceManager _service;
        public RolePermissionController(IServiceManager service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetRolePermissions()
        {
            var RolePermissions = await _service.RolePermissionService.GetAllRolePermissionsAsync(trackChanges: false);

            return Ok(RolePermissions);
        }

        [HttpGet("{id:guid}", Name = "RolePermissionById")]
        public async Task<IActionResult> GetRolePermission(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var RolePermission = await _service.RolePermissionService.GetRolePermissionAsync(id, trackChanges: false);
            return Ok(RolePermission);
        }


        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateRolePermission([FromBody] RolePermissionForCreationDto RolePermission)
        {
            var createdRolePermission = await _service.RolePermissionService.CreateRolePermissionAsync(RolePermission);

            return CreatedAtRoute("RolePermissionById", new { id = createdRolePermission.Id }, createdRolePermission);
        }


        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteRolePermission(Guid id)
        {
            await _service.RolePermissionService.DeleteRolePermissionAsync(id, trackChanges: false);

            return NoContent();
        }

        [HttpPut("{id:guid}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateRolePermission(Guid id, [FromBody] RolePermissionForUpdateDto RolePermission)
        {
            await _service.RolePermissionService.UpdateRolePermissionAsync(id, RolePermission, trackChanges: true);

            return NoContent();
        }
        [HttpPost("assign")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> AssignPermissionsToRole([FromBody] RolePermissionForAssignmentDto assignmentDto)
        {
            //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //if (!await _service.AuthorizationService.HasPermission(userId, "RolePermissions", "Create"))
            //{
            //    return Forbid();
            //}
            await _service.RolePermissionService.AssignPermissionsToRoleAsync(assignmentDto);
            return Ok(new { Message = "Permissions assigned successfully" });
        }

        [HttpGet("role/{role}")]
        public async Task<IActionResult> GetRolePermissionsByRoleId(string role)
        {
            var rolePermissions = await _service.RolePermissionService.GetRolePermissionsByRoleIdAsync(role, false);
            return Ok(rolePermissions);
        }
        [HttpGet("role-map-permissions/{role}")]
        public async Task<IActionResult> GetRolePermissionsByRoleName(string role)
        {
            var rolePermissions = await _service.RolePermissionService.GetRolePermissionsByRoleNameAsync(role, false);
            return Ok(rolePermissions);
        }

    }
}
