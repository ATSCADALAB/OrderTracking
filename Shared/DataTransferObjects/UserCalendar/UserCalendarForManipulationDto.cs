namespace Shared.DataTransferObjects.UserCalendar
{
    public record UserCalendarForManipulationDto
    {
        public string? Name { get; set; }
        public string? UserId { get; set; }
    }
}