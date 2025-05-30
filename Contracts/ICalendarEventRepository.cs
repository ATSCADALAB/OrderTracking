namespace Contracts
{
    using Entities.Models;

    public interface ICalendarEventRepository
    {
        Task<CalendarEvent?> GetByOrderCodeAsync(string orderCode);
        Task<bool> ExistsAsync(string orderCode);
        void CreateCalendarEvent(CalendarEvent order);
    }
}