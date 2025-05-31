using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Repository
{
    internal sealed class CalendarEventRepository : RepositoryBase<CalendarEvent>, ICalendarEventRepository
    {
        public CalendarEventRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }
        public async Task<IEnumerable<CalendarEvent>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await FindByCondition(x => x.StartDate >= startDate && x.StartDate < endDate, false)
                .ToListAsync();
        }
        public async Task<CalendarEvent?> GetByOrderCodeAsync(string orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
            {
                return null;
            }

            orderCode = orderCode.Trim();
            if (!Regex.IsMatch(orderCode, @"^[a-zA-Z0-9-]+$"))
            {
                return null;
            }

            return await FindByCondition(x => x.OrderCode==orderCode, false)
                .SingleOrDefaultAsync();
        }
        public async Task<bool> ExistsAsync(string orderCode) =>
            await FindByCondition(x => x.OrderCode == orderCode, false)
                .AnyAsync();

        public void CreateCalendarEvent(CalendarEvent order)
        {
            Create(order);
        }     
    }
}
