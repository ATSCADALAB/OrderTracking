using Entities.Models;

namespace Contracts
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync(bool trackChanges);
        Task<Employee?> GetEmployeeByIdAsync(Guid employeeId, bool trackChanges);
        Task<Employee?> GetEmployeeByCodeAsync(string employeeCode, bool trackChanges);
        Task<Employee?> GetEmployeeByEmailAsync(string email, bool trackChanges);
        Task<IEnumerable<Employee>> GetEmployeesByStatusAsync(EmployeeStatus status, bool trackChanges);
        Task<IEnumerable<Employee>> GetEmployeesWithUpcomingProbationEndAsync(int daysBefore, bool trackChanges);
        Task<IEnumerable<Employee>> GetEmployeesWithUpcomingFirstYearEndAsync(int daysBefore, bool trackChanges);
        void CreateEmployee(Employee employee);
        void UpdateEmployee(Employee employee);
        void DeleteEmployee(Employee employee);
        Task<bool> EmployeeCodeExistsAsync(string employeeCode);
        Task<bool> EmailExistsAsync(string email);
    }
}