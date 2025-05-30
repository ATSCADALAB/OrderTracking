namespace Entities.Exceptions.UserCalendar
{
    public sealed class UserCalendarNotFoundExceptionr : NotFoundException
    {
        public UserCalendarNotFoundExceptionr(Guid User)
            : base($"The Category with id: {User} doesn't exist in the database.")
        {
        }
    }
}
