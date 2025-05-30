using Entities.Models;
using Shared.DataTransferObjects.CalendarEvent;

namespace Service.Contracts
{
    public interface ICalendarEventService
    {
        Task<CalendarEventDto?> GetByOrderCodeAsync(string orderCode, bool trackChanges);
        Task CreateIfNotExistsAsync(CalendarEventDto dto);
    }
}
