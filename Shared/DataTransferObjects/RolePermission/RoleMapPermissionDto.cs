using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.DataTransferObjects.RolePermission
{
    public record RoleMapPermissionDto
    {
        public string PermissionName { get; set; }

        public string CategoryName { get; set; }
    }
}
