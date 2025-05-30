using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.DataTransferObjects.RolePermission
{
    public record RolePermissionDto
    {
        public Guid Id { get; set; }
        public string RoleId { get; set; } = default!;

        public Guid PermissionId { get; set; }

        public Guid CategoryId { get; set; }
    }
}
