namespace Shared.DataTransferObjects.UserCalendar
{
    public record UserCalendarDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? UserName { get; set; }
    }
}
