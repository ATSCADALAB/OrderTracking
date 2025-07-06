using AutoMapper;
using Contracts;
using Entities.Exceptions.Employee;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects.Employee;

namespace Service
{
    internal sealed class EmployeeService : IEmployeeService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public EmployeeService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync(bool trackChanges)
        {
            var employees = await _repository.Employee.GetAllEmployeesAsync(trackChanges);
            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employees);
            return employeesDto;
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(Guid employeeId, bool trackChanges)
        {
            var employee = await GetEmployeeAndCheckIfItExists(employeeId, trackChanges);
            var employeeDto = _mapper.Map<EmployeeDto>(employee);
            return employeeDto;
        }

        public async Task<EmployeeDto?> GetEmployeeByCodeAsync(string employeeCode, bool trackChanges)
        {
            var employee = await _repository.Employee.GetEmployeeByCodeAsync(employeeCode, trackChanges);
            if (employee == null) return null;

            var employeeDto = _mapper.Map<EmployeeDto>(employee);
            return employeeDto;
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesByStatusAsync(int status, bool trackChanges)
        {
            var employeeStatus = (EmployeeStatus)status;
            var employees = await _repository.Employee.GetEmployeesByStatusAsync(employeeStatus, trackChanges);
            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employees);
            return employeesDto;
        }

        public async Task<EmployeeDto> CreateEmployeeAsync(EmployeeForCreationDto employeeForCreation, string createdBy)
        {
            // Kiểm tra mã nhân viên đã tồn tại
            if (await _repository.Employee.EmployeeCodeExistsAsync(employeeForCreation.EmployeeCode))
                throw new EmployeeCodeAlreadyExistsException(employeeForCreation.EmployeeCode);

            // Kiểm tra email đã tồn tại
            if (await _repository.Employee.EmailExistsAsync(employeeForCreation.Email))
                throw new EmployeeEmailAlreadyExistsException(employeeForCreation.Email);

            var employee = _mapper.Map<Employee>(employeeForCreation);
            employee.Id = Guid.NewGuid();
            employee.CreatedAt = DateTime.UtcNow;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.CreatedBy = createdBy;
            employee.Status = EmployeeStatus.Probation;

            // Tính toán ngày kết thúc (sẽ implement logic tính toán ngày làm việc sau)
            await CalculateEmployeeDatesInternalAsync(employee);

            _repository.Employee.CreateEmployee(employee);
            await _repository.SaveAsync();

            var employeeDto = _mapper.Map<EmployeeDto>(employee);
            return employeeDto;
        }

        public async Task UpdateEmployeeAsync(Guid employeeId, EmployeeForUpdateDto employeeForUpdate, string updatedBy, bool trackChanges)
        {
            var employee = await GetEmployeeAndCheckIfItExists(employeeId, trackChanges);

            // Kiểm tra email đã tồn tại (trừ chính employee này)
            var existingEmailEmployee = await _repository.Employee.GetEmployeeByEmailAsync(employeeForUpdate.Email, false);
            if (existingEmailEmployee != null && existingEmailEmployee.Id != employeeId)
                throw new EmployeeEmailAlreadyExistsException(employeeForUpdate.Email);

            _mapper.Map(employeeForUpdate, employee);
            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = updatedBy;

            await _repository.SaveAsync();
        }

        public async Task DeleteEmployeeAsync(Guid employeeId, bool trackChanges)
        {
            var employee = await GetEmployeeAndCheckIfItExists(employeeId, trackChanges);

            _repository.Employee.DeleteEmployee(employee);
            await _repository.SaveAsync();
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesWithUpcomingProbationEndAsync(int daysBefore, bool trackChanges)
        {
            var employees = await _repository.Employee.GetEmployeesWithUpcomingProbationEndAsync(daysBefore, trackChanges);
            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employees);
            return employeesDto;
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesWithUpcomingFirstYearEndAsync(int daysBefore, bool trackChanges)
        {
            var employees = await _repository.Employee.GetEmployeesWithUpcomingFirstYearEndAsync(daysBefore, trackChanges);
            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employees);
            return employeesDto;
        }

        public async Task CalculateEmployeeDatesAsync(Guid employeeId, bool trackChanges)
        {
            var employee = await GetEmployeeAndCheckIfItExists(employeeId, trackChanges);
            await CalculateEmployeeDatesInternalAsync(employee);
            await _repository.SaveAsync();
        }

        // Private methods
        private async Task<Employee> GetEmployeeAndCheckIfItExists(Guid id, bool trackChanges)
        {
            var employee = await _repository.Employee.GetEmployeeByIdAsync(id, trackChanges);
            if (employee is null)
                throw new EmployeeNotFoundException(id);

            return employee;
        }

        private async Task CalculateEmployeeDatesInternalAsync(Employee employee)
        {
            // Tính ngày kết thúc thử việc: 60 ngày làm việc
            // Logic tạm thời - sẽ được thay thế bằng WorkingDayCalculationService
            employee.ProbationEndDate = CalculateWorkingDaysFromStart(employee.StartDate, 60);

            // Tính ngày kết thúc năm đầu: 365 ngày từ ngày bắt đầu
            employee.FirstYearEndDate = employee.StartDate.AddYears(1);
        }

        private DateTime CalculateWorkingDaysFromStart(DateTime startDate, int workingDays)
        {
            // Logic tạm thời: Tính thô 
            // Giả sử làm 6 ngày/tuần (T2-T7) = 60 ngày làm việc ≈ 10 tuần = 70 ngày calendar
            var estimatedDays = (int)(workingDays * 7.0 / 6.0); // 6 ngày làm việc/tuần
            return startDate.AddDays(estimatedDays);
        }
        public async Task UpdateEmployeeStatusAsync(Guid employeeId, int status, string? notes, string updatedBy, bool trackChanges)
        {
            var employee = await GetEmployeeAndCheckIfItExists(employeeId, trackChanges);

            employee.Status = (EmployeeStatus)status;
            if (!string.IsNullOrEmpty(notes))
                employee.Notes = notes;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = updatedBy;

            await _repository.SaveAsync();
        }
    }
}
