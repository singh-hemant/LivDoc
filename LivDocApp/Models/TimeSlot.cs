using System.ComponentModel.DataAnnotations;

namespace LivDocApp.Models
{
    public class TimeSlot
    {
        [Key]
        public int TimeSlotID { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }
    }
}
