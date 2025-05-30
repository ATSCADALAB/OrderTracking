namespace Shared.DataTransferObjects.Permission
{
    public abstract record PermissionForManipulationDto
    {
        public string? Name { get; set; }
    }
}
