namespace LivDocApp.Models
{
    public class BookingListModel
    {
        public List<Appointment> Appointments { get; set; }
        public List<Doctor> Doctors { get; set; }
        public List<TimeSpan> TimeSlots { get; set; }

    }
}
