namespace Entities.Exceptions.Permission
{
    public sealed class PermissionNotFoundException : NotFoundException
    {
        public PermissionNotFoundException(Guid customerId)
            : base($"The customer with id: {customerId} doesn't exist in the database.")
        {
        }
    }
}
