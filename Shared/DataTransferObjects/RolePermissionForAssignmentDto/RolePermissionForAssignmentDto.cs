namespace QuickStart.Shared.DataTransferObjects.RolePermission
{
    public class RolePermissionForAssignmentDto
    {
        public string RoleId { get; set; } = default!;
        public List<CategoryPermissionAssignmentDto> CategoryAssignments { get; set; } = new List<CategoryPermissionAssignmentDto>();
    }

    public class CategoryPermissionAssignmentDto
    {
        public Guid CategoryId { get; set; }
        public List<Guid> PermissionIds { get; set; } = new List<Guid>();
    }
}