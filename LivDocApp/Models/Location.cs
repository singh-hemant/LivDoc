using System.ComponentModel.DataAnnotations;

namespace LivDocApp.Models
{
    public class Location
    {
        [Key]
        public int LocationId { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }
        [Required]
        public string Country { get; set; }
    }
}
