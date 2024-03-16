using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LivDocApp.Models
{
    public class Hospital
    {
        
        [Key]
        public int HospitalID { get; set; }

        [Required]
        public string Name { get; set; }

        [ForeignKey("Location")]
        public int LocationID { get; set; }

        public virtual Location Location { get; set; }
    }

}

