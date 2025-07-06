namespace Entities.Exceptions.Employee
{
    public sealed class EmployeeCodeAlreadyExistsException : BadRequestException
    {
        public EmployeeCodeAlreadyExistsException(string employeeCode)
            : base($"Mã nhân viên '{employeeCode}' đã tồn tại trong hệ thống.")
        {
        }
    }
}