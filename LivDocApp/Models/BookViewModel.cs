﻿namespace LivDocApp.Models
{
    public class BookViewModel
    {
        public Doctor Doctor { get; set; }
        public List<Appointment> Appointments { get; set; }

        public DateTime SelectedDate { get; set; }
        public List<TimeSpan> TimeSlots { get; set; }
    }
}
