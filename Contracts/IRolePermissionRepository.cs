using Entities.Models;

namespace Contracts
{
    public interface IRolePermissionRepository
    {
        Task<IEnumerable<RolePermission>> GetRolePermissionsAsync(bool trackChanges);
        Task<RolePermission> GetRolePermissionAsync(Guid RolePermissionId, bool trackChanges);
        void CreateRolePermission(RolePermission RolePermission);
        void DeleteRolePermission(RolePermission RolePermission);
        Task<List<RolePermission>> GetPermissionsByRolesAsync(List<string> roleNames, bool trackChanges);
        Task<IEnumerable<RolePermission>> GetRolePermissionsByRoleIdAsync(string roleId, bool trackChanges); // Phương thức này
        Task<IEnumerable<RolePermission>> GetRolePermissionsByRoleNameAsync(string roleName, bool trackChanges); // Phương thức này
        Task DeleteRolePermissionsByRoleIdAsync(string roleId); // Định nghĩa phương thức này
    }
}
