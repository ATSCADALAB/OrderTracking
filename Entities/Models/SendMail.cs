using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class SendMail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderCode { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } // "Pending", "Completed", "Failed"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        public string? ErrorMessage { get; set; }
    }
}