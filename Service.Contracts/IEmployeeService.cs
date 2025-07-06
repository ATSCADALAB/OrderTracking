using Shared.DataTransferObjects.Employee;

namespace Service.Contracts
{
    public interface IEmployeeService
    {
        Task UpdateEmployeeStatusAsync(Guid employeeId, int status, string? notes, string updatedBy, bool trackChanges);
        Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync(bool trackChanges);
        Task<EmployeeDto?> GetEmployeeByIdAsync(Guid employeeId, bool trackChanges);
        Task<EmployeeDto?> GetEmployeeByCodeAsync(string employeeCode, bool trackChanges);
        Task<IEnumerable<EmployeeDto>> GetEmployeesByStatusAsync(int status, bool trackChanges);
        Task<EmployeeDto> CreateEmployeeAsync(EmployeeForCreationDto employeeForCreation, string createdBy);
        Task UpdateEmployeeAsync(Guid employeeId, EmployeeForUpdateDto employeeForUpdate, string updatedBy, bool trackChanges);
        Task DeleteEmployeeAsync(Guid employeeId, bool trackChanges);
        Task<IEnumerable<EmployeeDto>> GetEmployeesWithUpcomingProbationEndAsync(int daysBefore, bool trackChanges);
        Task<IEnumerable<EmployeeDto>> GetEmployeesWithUpcomingFirstYearEndAsync(int daysBefore, bool trackChanges);
        Task CalculateEmployeeDatesAsync(Guid employeeId, bool trackChanges);
    }
}