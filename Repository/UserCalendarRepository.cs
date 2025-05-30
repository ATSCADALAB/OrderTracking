using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    internal sealed class UserCalendarRepository : RepositoryBase<UserCalendar>, IUserCalendarRepository
    {
        public UserCalendarRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }

        public void CreateUserCalendar(UserCalendar userCalendar)
        {
            Create(userCalendar);
        }

        public void DeleteUserCalendar(UserCalendar userCalendar)
        {
            Delete(userCalendar);
        }

        public async Task<UserCalendar> GetUserCalendarAsync(Guid userCalendarId, bool trackChanges)
        {
            return await FindByCondition(uc => uc.Id.Equals(userCalendarId), trackChanges)
                .Include(u => u.User)
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<UserCalendar>> GetUserCalendarsAsync(bool trackChanges)
        {
            return await FindAll(trackChanges)
                .Include(u => u.User)
                .ToListAsync();
        }
    }
}
