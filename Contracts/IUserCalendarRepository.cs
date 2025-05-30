using Entities.Models;

namespace Contracts
{
    public interface IUserCalendarRepository
    {
        Task<IEnumerable<UserCalendar>> GetUserCalendarsAsync(bool trackChanges);
        Task<UserCalendar> GetUserCalendarAsync(Guid userCalendarId, bool trackChanges);
        void CreateUserCalendar(UserCalendar userCalendar);
        void DeleteUserCalendar(UserCalendar userCalendar);
    }
}
