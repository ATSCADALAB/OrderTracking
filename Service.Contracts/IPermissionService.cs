using Shared.DataTransferObjects.Permission;

namespace Service.Contracts
{
    public interface IPermissionService
    {
        Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync(bool trackChanges);
        Task<PermissionDto> GetPermissionAsync(Guid PermissionId, bool trackChanges);
        Task<PermissionDto> CreatePermissionAsync(PermissionForCreationDto Permission);
        Task DeletePermissionAsync(Guid PermissionId, bool trackChanges);
        Task UpdatePermissionAsync(Guid PermissionId, PermissionForUpdateDto PermissionForUpdate, bool trackChanges);
    }
}
