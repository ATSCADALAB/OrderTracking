using Entities.Models;

namespace Contracts
{
    public interface IPermissionRepository
    {
        Task<IEnumerable<Permission>> GetPermissionsAsync(bool trackChanges);
        Task<Permission> GetPermissionAsync(Guid PermissionId, bool trackChanges);
        void CreatePermission(Permission Permission);
        void DeletePermission(Permission Permission);
    }
}
