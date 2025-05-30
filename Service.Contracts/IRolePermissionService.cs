using QuickStart.Shared.DataTransferObjects.RolePermission;
using Shared.DataTransferObjects.RolePermission;

namespace Service.Contracts
{
    public interface IRolePermissionService
    {
        Task<IEnumerable<RolePermissionDto>> GetAllRolePermissionsAsync(bool trackChanges);
        Task<RolePermissionDto> GetRolePermissionAsync(Guid RolePermissionId, bool trackChanges);
        Task<RolePermissionDto> CreateRolePermissionAsync(RolePermissionForCreationDto RolePermission);
        Task DeleteRolePermissionAsync(Guid RolePermissionId, bool trackChanges);
        Task UpdateRolePermissionAsync(Guid RolePermissionId, RolePermissionForUpdateDto RolePermissionForUpdate, bool trackChanges);
        Task AssignPermissionsToRoleAsync(RolePermissionForAssignmentDto assignmentDto);
        Task<IEnumerable<RolePermissionDto>> GetRolePermissionsByRoleIdAsync(string roleId, bool trackChanges); // Phương thức này
        Task<IEnumerable<RoleMapPermissionDto>> GetRolePermissionsByRoleNameAsync(string roleId, bool trackChanges); // Phương thức này
    }
}
