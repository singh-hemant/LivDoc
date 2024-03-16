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
        public int SpecialtyID { get; set; }
        public virtual Specialty Specialty { get; set; }

        [ForeignKey("Hospital")]
        public int HospitalID { get; set; }
        public virtual Hospital Hospital { get; set; }

        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public string DocImgURL { get; set; }
        
    }
}
