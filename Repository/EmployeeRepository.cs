using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    internal sealed class EmployeeRepository : RepositoryBase<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync(bool trackChanges) =>
            await FindAll(trackChanges)
                .OrderBy(e => e.FullName)
                .ToListAsync();

        public async Task<Employee?> GetEmployeeByIdAsync(Guid employeeId, bool trackChanges) =>
            await FindByCondition(e => e.Id.Equals(employeeId), trackChanges)
                .SingleOrDefaultAsync();

        public async Task<Employee?> GetEmployeeByCodeAsync(string employeeCode, bool trackChanges) =>
            await FindByCondition(e => e.EmployeeCode.Equals(employeeCode), trackChanges)
                .SingleOrDefaultAsync();

        public async Task<Employee?> GetEmployeeByEmailAsync(string email, bool trackChanges) =>
            await FindByCondition(e => e.Email.Equals(email), trackChanges)
                .SingleOrDefaultAsync();

        public async Task<IEnumerable<Employee>> GetEmployeesByStatusAsync(EmployeeStatus status, bool trackChanges) =>
            await FindByCondition(e => e.Status == status, trackChanges)
                .OrderBy(e => e.FullName)
                .ToListAsync();

        public async Task<IEnumerable<Employee>> GetEmployeesWithUpcomingProbationEndAsync(int daysBefore, bool trackChanges)
        {
            var targetDate = DateTime.Today.AddDays(daysBefore);
            return await FindByCondition(e => e.Status == EmployeeStatus.Probation &&
                                            e.ProbationEndDate.HasValue &&
                                            e.ProbationEndDate.Value.Date == targetDate, trackChanges)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetEmployeesWithUpcomingFirstYearEndAsync(int daysBefore, bool trackChanges)
        {
            var targetDate = DateTime.Today.AddDays(daysBefore);
            return await FindByCondition(e => e.Status == EmployeeStatus.Official &&
                                            e.FirstYearEndDate.HasValue &&
                                            e.FirstYearEndDate.Value.Date == targetDate, trackChanges)
                .ToListAsync();
        }

        public void CreateEmployee(Employee employee) => Create(employee);

        public void UpdateEmployee(Employee employee) => Update(employee);

        public void DeleteEmployee(Employee employee) => Delete(employee);

        public async Task<bool> EmployeeCodeExistsAsync(string employeeCode) =>
            await FindByCondition(e => e.EmployeeCode.Equals(employeeCode), false)
                .AnyAsync();

        public async Task<bool> EmailExistsAsync(string email) =>
            await FindByCondition(e => e.Email.Equals(email), false)
                .AnyAsync();
    }
}