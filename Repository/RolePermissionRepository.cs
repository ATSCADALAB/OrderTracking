using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    internal sealed class RolePermissionRepository : RepositoryBase<RolePermission>, IRolePermissionRepository
    {
        public RolePermissionRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }
        public async Task<List<RolePermission>> GetPermissionsByRolesAsync(List<string> roleNames, bool trackChanges)
        {
            return await FindAll(trackChanges)
                .Include(rp => rp.Permission)
                .Include(rp => rp.Category)
                .Where(rp => roleNames.Contains(rp.Role.Name))
                .ToListAsync();
        }
        public void CreateRolePermission(RolePermission RolePermission)
        {
            Create(RolePermission);
        }

        public void DeleteRolePermission(RolePermission RolePermission)
        {
            Delete(RolePermission);
        }

        public async Task<RolePermission> GetRolePermissionAsync(Guid RolePermissionId, bool trackChanges)
        {
            return await FindByCondition(c => c.Id.Equals(RolePermissionId), trackChanges)
 
                .SingleOrDefaultAsync();

        }
        public async Task DeleteRolePermissionsByRoleIdAsync(string roleId)
        {
            var rolePermissions = await FindByCondition(rp => rp.RoleId == roleId, false)
                .ToListAsync();
            foreach (var rolePermission in rolePermissions)
            {
                Delete(rolePermission);
            }
        }
        public async Task<IEnumerable<RolePermission>> GetRolePermissionsAsync(bool trackChanges)
        {
            return await FindAll(trackChanges)
                .Include(c => c.Category)
                .Include(c => c.Permission)
                .ToListAsync();
        }
        public async Task<IEnumerable<RolePermission>> GetRolePermissionsByRoleIdAsync(string roleId, bool trackChanges)
        {
            return await FindByCondition(rp => rp.RoleId == roleId, trackChanges)
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .Include(rp => rp.Category)
                .ToListAsync();
        }
        public async Task<IEnumerable<RolePermission>> GetRolePermissionsByRoleNameAsync(string roleId, bool trackChanges)
        {
            return await FindByCondition(rp => rp.Role.Name == roleId, trackChanges)
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .Include(rp => rp.Category)
                .ToListAsync();
        }
    }
}
