namespace Shared.DataTransferObjects.CalendarEvent
{
    public record CalendarEventDto
    {
        public string Title { get; init; }
        public string OrderCode { get; init; }
        public string? Description { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public string? UserName { get; init; }
    }
}