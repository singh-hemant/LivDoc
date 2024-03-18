using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LivDocApp.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Experience { get; set; }

        [ForeignKey("Specialty")]
        [Required(ErrorMessage = "Specialty is required")]
        public int SpecialtyID { get; set; }
        public virtual required Specialty Specialty { get; set; }

        [ForeignKey("Hospital")]
        [Required(ErrorMessage = "Hospital is required")]
        public int HospitalID { get; set; }
        public virtual required Hospital Hospital { get; set; }

        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public string? DocImgURL { get; set; }
        
    }
}
