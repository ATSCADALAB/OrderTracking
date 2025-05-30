using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class CalendarEvent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        [StringLength(255)]
        public string OrderCode { get; set; }

        [StringLength(2000)] // Description thường dài hơn nên để 2000 ký tự
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [StringLength(255)]
        public string? UserName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}