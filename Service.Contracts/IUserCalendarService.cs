using Shared.DataTransferObjects.UserCalendar;

namespace Service.Contracts
{
    public interface IUserCalendarService
    {
        Task<IEnumerable<UserCalendarDto>> GetAllUserCalendarsAsync(bool trackChanges);
        Task<UserCalendarDto> GetUserCalendarAsync(Guid userCalendarId, bool trackChanges);
        Task<UserCalendarDto> CreateUserCalendarAsync(UserCalendarForCreationDto userCalendarDto);
        Task DeleteUserCalendarAsync(Guid userCalendarId, bool trackChanges);
    }
}
