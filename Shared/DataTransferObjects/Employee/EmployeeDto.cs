using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObjects.Employee
{
    public record EmployeeDto
    {
        public Guid Id { get; init; }
        public string EmployeeCode { get; init; } = default!;
        public string FullName { get; init; } = default!;
        public string Email { get; init; } = default!;
        public string? PhoneNumber { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime? ProbationEndDate { get; init; }
        public DateTime? FirstYearEndDate { get; init; }
        public string? Position { get; init; }
        public string? Department { get; init; }
        public decimal? Salary { get; init; }
        public string Status { get; init; } = default!;
        public string? Notes { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string? CreatedBy { get; init; }
        public string? UpdatedBy { get; init; }
    }

    public record EmployeeForCreationDto
    {
        [Required(ErrorMessage = "Mã nhân viên là bắt buộc")]
        [StringLength(50, ErrorMessage = "Mã nhân viên không được quá 50 ký tự")]
        public string EmployeeCode { get; init; } = default!;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự")]
        public string FullName { get; init; } = default!;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(200, ErrorMessage = "Email không được quá 200 ký tự")]
        public string Email { get; init; } = default!;

        [StringLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
        public string? PhoneNumber { get; init; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        public DateTime StartDate { get; init; }

        [StringLength(100, ErrorMessage = "Chức vụ không được quá 100 ký tự")]
        public string? Position { get; init; }

        [StringLength(100, ErrorMessage = "Phòng ban không được quá 100 ký tự")]
        public string? Department { get; init; }

        public decimal? Salary { get; init; }

        public string? Notes { get; init; }
    }

    public record EmployeeForUpdateDto
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự")]
        public string FullName { get; init; } = default!;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(200, ErrorMessage = "Email không được quá 200 ký tự")]
        public string Email { get; init; } = default!;

        [StringLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
        public string? PhoneNumber { get; init; }

        [StringLength(100, ErrorMessage = "Chức vụ không được quá 100 ký tự")]
        public string? Position { get; init; }

        [StringLength(100, ErrorMessage = "Phòng ban không được quá 100 ký tự")]
        public string? Department { get; init; }

        public decimal? Salary { get; init; }

        public int Status { get; init; }

        public string? Notes { get; init; }
    }
    public record EmployeeStatusUpdateDto
    {
        /// <summary>
        /// Trạng thái mới: 1=Probation, 2=Official, 3=Terminated, 4=Resigned
        /// </summary>
        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [Range(1, 4, ErrorMessage = "Trạng thái phải từ 1-4")]
        public int Status { get; init; }

        /// <summary>
        /// Ghi chú thêm (tùy chọn)
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự")]
        public string? Notes { get; init; }
    }
}