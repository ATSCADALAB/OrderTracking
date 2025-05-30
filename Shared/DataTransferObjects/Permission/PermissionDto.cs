namespace Shared.DataTransferObjects.Permission
{
    public record PermissionDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }
}
