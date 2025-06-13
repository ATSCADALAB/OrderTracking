using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class EmailCcConfiguration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ConfigKey { get; set; } // Ví dụ: "OrderTracking", "General", "SystemNotification"

        [Required]
        [StringLength(200)]
        public string ConfigName { get; set; } // Tên hiển thị

        [Required]
        public bool IsEnabled { get; set; } = true; // Bật/tắt

        [StringLength(500)]
        public string? Description { get; set; }

        // Email CC settings
        public string? DefaultCcEmails { get; set; } // Lưu dạng JSON array
        public string? DefaultBccEmails { get; set; } // Lưu dạng JSON array

        // Specific settings
        public bool AutoAddCcForOrders { get; set; } = false;
        public bool AutoAddCcForNotifications { get; set; } = false;
        public bool AutoAddCcForAlerts { get; set; } = false;

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        // Priority for ordering
        public int Priority { get; set; } = 1;
    }
}