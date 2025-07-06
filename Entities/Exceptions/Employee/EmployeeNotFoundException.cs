namespace Entities.Exceptions.Employee
{
    public sealed class EmployeeNotFoundException : NotFoundException
    {
        public EmployeeNotFoundException(Guid employeeId)
            : base($"Nhân viên với ID: {employeeId} không tồn tại trong hệ thống.")
        {
        }
    }
}