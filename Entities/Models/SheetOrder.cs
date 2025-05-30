using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class SheetOrder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string OrderCode { get; set; }

        [StringLength(255)]
        public string? OrderName { get; set; }

        [StringLength(255)]
        public string? Dev1 { get; set; }

        [StringLength(255)]
        public string? QC { get; set; }

        [StringLength(255)]
        public string? Code { get; set; }

        [StringLength(255)]
        public string? Sale { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(255)]
        public string? Status { get; set; }

        [StringLength(255)]
        public string? SheetName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}