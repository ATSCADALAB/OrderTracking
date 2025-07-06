using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    public class Employee
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string EmployeeCode { get; set; } = default!;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = default!;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = default!;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? ProbationEndDate { get; set; }

        public DateTime? FirstYearEndDate { get; set; }

        [StringLength(100)]
        public string? Position { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        public decimal? Salary { get; set; }

        [Required]
        public EmployeeStatus Status { get; set; } = EmployeeStatus.Probation;

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        [StringLength(100)]
        public string? UpdatedBy { get; set; }
    }

    public enum EmployeeStatus
    {
        Probation = 1,      // Thử việc
        Official = 2,       // Chính thức
        Terminated = 3,     // Đã nghỉ việc
        Resigned = 4        // Tự nghỉ việc
    }
}