using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security;

namespace Entities.Models
{
    public class Category
    {
        [Column("CategoryId")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(60, ErrorMessage = "Name can't be longer than 60 characters")]
        public string Name { get; set; } // "Customers", "Orders", "Products"

    }
}
