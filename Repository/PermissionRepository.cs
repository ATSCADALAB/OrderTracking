using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    internal sealed class PermissionRepository : RepositoryBase<Permission>, IPermissionRepository
    {
        public PermissionRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }

        public void CreatePermission(Permission Permission)
        {
            Create(Permission);
        }

        public void DeletePermission(Permission Permission)
        {
            Delete(Permission);
        }

        public async Task<Permission> GetPermissionAsync(Guid PermissionId, bool trackChanges)
        {
            return await FindByCondition(c => c.Id.Equals(PermissionId), trackChanges)
 
                .SingleOrDefaultAsync();

        }

        public async Task<IEnumerable<Permission>> GetPermissionsAsync(bool trackChanges)
        {
            return await FindAll(trackChanges)
                .ToListAsync();
        }
    }
}
