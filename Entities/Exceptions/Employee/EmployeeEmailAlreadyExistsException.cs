namespace Entities.Exceptions.Employee
{
    public sealed class EmployeeEmailAlreadyExistsException : BadRequestException
    {
        public EmployeeEmailAlreadyExistsException(string email)
            : base($"Email '{email}' đã được sử dụng bởi nhân viên khác.")
        {
        }
    }
}