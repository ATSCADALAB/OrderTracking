namespace Entities.Exceptions.RolePermission
{
    public sealed class RolePermissionNotFoundException : NotFoundException
    {
        public RolePermissionNotFoundException(Guid customerId)
            : base($"The customer with id: {customerId} doesn't exist in the database.")
        {
        }
    }
}
