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

        [Required]
        public DateTime AppointmentDate { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorID { get; set; }
        public virtual Doctor Doctor { get; set; }

        [ForeignKey("TimeSlot")]
        public int TimeSlotID { get; set; }
        public virtual TimeSlot TimeSlot { get; set; }

        

        
        

    }
}
