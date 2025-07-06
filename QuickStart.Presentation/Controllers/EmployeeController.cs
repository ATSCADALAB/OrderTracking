using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickStart.Presentation.ActionFilters;
using Service.Contracts;
using Shared.DataTransferObjects.Employee;
using System.Security.Claims;
using Entities.Models;
namespace QuickStart.Presentation.Controllers
{
    [Route("api/employees")]
    [ApiController]
    [Authorize] // Yêu cầu đăng nhập
    public class EmployeeController : ControllerBase
    {
        private readonly IServiceManager _service;

        public EmployeeController(IServiceManager service) => _service = service;

        /// <summary>
        /// Lấy danh sách tất cả nhân viên
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var employees = await _service.EmployeeService.GetAllEmployeesAsync(trackChanges: false);
            return Ok(employees);
        }

        /// <summary>
        /// Lấy thông tin nhân viên theo ID
        /// </summary>
        [HttpGet("{id:guid}", Name = "EmployeeById")]
        public async Task<IActionResult> GetEmployee(Guid id)
        {
            var employee = await _service.EmployeeService.GetEmployeeByIdAsync(id, trackChanges: false);
            if (employee == null)
                return NotFound($"Không tìm thấy nhân viên với ID: {id}");

            return Ok(employee);
        }

        /// <summary>
        /// Lấy thông tin nhân viên theo mã nhân viên
        /// </summary>
        [HttpGet("code/{employeeCode}")]
        public async Task<IActionResult> GetEmployeeByCode(string employeeCode)
        {
            var employee = await _service.EmployeeService.GetEmployeeByCodeAsync(employeeCode, trackChanges: false);
            if (employee == null)
                return NotFound($"Không tìm thấy nhân viên với mã: {employeeCode}");

            return Ok(employee);
        }

        /// <summary>
        /// Lấy danh sách nhân viên theo trạng thái
        /// </summary>
        [HttpGet("status/{status:int}")]
        public async Task<IActionResult> GetEmployeesByStatus(int status)
        {
            if (!Enum.IsDefined(typeof(EmployeeStatus), status))
                return BadRequest("Trạng thái không hợp lệ");

            var employees = await _service.EmployeeService.GetEmployeesByStatusAsync(status, trackChanges: false);
            return Ok(employees);
        }

        /// <summary>
        /// Tạo nhân viên mới
        /// </summary>
        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeForCreationDto employee)
        {
            var currentUser = GetCurrentUser();
            var createdEmployee = await _service.EmployeeService.CreateEmployeeAsync(employee, currentUser);

            return CreatedAtRoute("EmployeeById", new { id = createdEmployee.Id }, createdEmployee);
        }

        /// <summary>
        /// Cập nhật thông tin nhân viên
        /// </summary>
        [HttpPut("{id:guid}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] EmployeeForUpdateDto employee)
        {
            var currentUser = GetCurrentUser();
            await _service.EmployeeService.UpdateEmployeeAsync(id, employee, currentUser, trackChanges: true);

            return NoContent();
        }

        /// <summary>
        /// Xóa nhân viên
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới được xóa
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            await _service.EmployeeService.DeleteEmployeeAsync(id, trackChanges: false);
            return NoContent();
        }

        /// <summary>
        /// Lấy danh sách nhân viên sắp hết hạn thử việc
        /// </summary>
        [HttpGet("upcoming-probation-end")]
        public async Task<IActionResult> GetUpcomingProbationEnd([FromQuery] int daysBefore = 3)
        {
            var employees = await _service.EmployeeService.GetEmployeesWithUpcomingProbationEndAsync(daysBefore, trackChanges: false);
            return Ok(employees);
        }

        /// <summary>
        /// Lấy danh sách nhân viên sắp hết năm đầu tiên
        /// </summary>
        [HttpGet("upcoming-first-year-end")]
        public async Task<IActionResult> GetUpcomingFirstYearEnd([FromQuery] int daysBefore = 7)
        {
            var employees = await _service.EmployeeService.GetEmployeesWithUpcomingFirstYearEndAsync(daysBefore, trackChanges: false);
            return Ok(employees);
        }

        /// <summary>
        /// Tính lại ngày kết thúc cho nhân viên (dùng khi thay đổi cấu hình)
        /// </summary>
        [HttpPost("{id:guid}/recalculate-dates")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RecalculateEmployeeDates(Guid id)
        {
            await _service.EmployeeService.CalculateEmployeeDatesAsync(id, trackChanges: true);
            return Ok(new { message = "Đã tính lại ngày kết thúc thành công" });
        }

        /// <summary>
        /// Cập nhật trạng thái nhân viên
        /// </summary>
        [HttpPatch("{id:guid}/status")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateEmployeeStatus(Guid id, [FromBody] EmployeeStatusUpdateDto statusUpdate)
        {
            if (!Enum.IsDefined(typeof(EmployeeStatus), statusUpdate.Status))
                return BadRequest("Trạng thái không hợp lệ");

            var updateDto = new EmployeeForUpdateDto
            {
                Status = statusUpdate.Status,
                Notes = statusUpdate.Notes,
                // Các field khác sẽ được giữ nguyên trong service
                FullName = "", // Sẽ được handle trong service
                Email = "" // Sẽ được handle trong service
            };

            var currentUser = GetCurrentUser();
            await _service.EmployeeService.UpdateEmployeeStatusAsync(id, statusUpdate.Status, statusUpdate.Notes, currentUser, trackChanges: true);

            return NoContent();
        }

        // Helper method để lấy user hiện tại
        private string GetCurrentUser()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ??
                   User.FindFirst(ClaimTypes.Email)?.Value ??
                   "System";
        }
    }
}