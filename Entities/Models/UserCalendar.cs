using Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    public class UserCalendar
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = default!;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = default!;
    }
}
