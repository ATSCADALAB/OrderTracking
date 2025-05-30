using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    public class RolePermission
    {
        [Column("RolePermissionId")]
        public Guid Id { get; set; }
        public string RoleId { get; set; } = default!;
        public IdentityRole Role { get; set; } = default!;

        public Guid PermissionId { get; set; }
        public Permission Permission { get; set; } = default!;

        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = default!;
    }
}
