using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LivDocApp.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentID { get; set; }
        [Required]
        public bool Status { get; set; }

     
        public string? PatientName { get; set; }
     
        public string? PatientEmail { get; set; }
      
        public string? PatientPhoneNumber { get; set; }

        [ForeignKey("Doctor")]
        public int? DoctorID { get; set; }
        public virtual Doctor Doctor { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly AppointmentDate { get; set; }

        [Required]
        
        public int TimeSlot { get; set; }

    }
}

