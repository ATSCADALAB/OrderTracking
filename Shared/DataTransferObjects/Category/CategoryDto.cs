namespace Shared.DataTransferObjects.Category
{
    public record CategoryDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }
}
