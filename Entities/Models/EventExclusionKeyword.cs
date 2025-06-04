using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class EventExclusionKeyword
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Keyword { get; set; } = default!;

        [StringLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}