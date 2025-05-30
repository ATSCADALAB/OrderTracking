namespace Shared.DataTransferObjects.RolePermission
{
    public abstract record RolePermissionForManipulationDto
    {
        public string RoleId { get; set; } = default!;

        public Guid PermissionId { get; set; }

        public Guid CategoryId { get; set; }
    }
}
