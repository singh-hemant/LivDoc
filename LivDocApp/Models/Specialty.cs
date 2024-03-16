using System.ComponentModel.DataAnnotations;

namespace LivDocApp.Models
{
    public class Specialty
    {
        [Key]
        public int SpecialtyId { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
