using LivDocApp.Data;
using LivDocApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;

namespace LivDocApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly DoctorsDbContext db;
        private readonly ApplicationDbContext UserDb;

        public HomeController(DoctorsDbContext context, ApplicationDbContext context1)
        {
            db = context;
            UserDb = context1;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
        public IActionResult Services()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult PageSearch()
        {
            return View();
        }



        public IActionResult Search(string searchTerm)
        {
            var query = from doctor in db.Doctors
                        join location in db.Locations on doctor.Hospital.LocationID equals location.LocationId into locationGroup
                        from loc in locationGroup.DefaultIfEmpty()
                        join hospital in db.Hospitals on doctor.HospitalID equals hospital.HospitalID into hospitalGroup
                        from hosp in hospitalGroup.DefaultIfEmpty()
                        join specialty in db.Specialties on doctor.SpecialtyID equals specialty.SpecialtyId into specialtyGroup
                        from spec in specialtyGroup.DefaultIfEmpty()
                        where doctor.Name.Contains(searchTerm) ||
                              loc.City.Contains(searchTerm) ||
                              hosp.Name.Contains(searchTerm) ||
                              spec.Name.Contains(searchTerm)
                        select new SearchResultViewModel
                        {
                            DoctorId = doctor.DoctorID,
                            DoctorName = doctor.Name,
                            LocationName = loc.City,
                            HospitalName = hosp.Name,
                            SpecialtyName = spec.Name
                        };


            List<SearchResultViewModel> results = query.ToList();

            return View("PageSearch", results);
        }
        [HttpGet]
        [Authorize]
        public IActionResult Book(int? id, DateTime date)
        {
            List<TimeSpan> timeSlots = new List<TimeSpan>();
            for (int i = 9; i < 35; i++)
            {
                // Assuming each integer represents an hour (e.g., 0 = 00:00, 1 = 01:00, etc.)
                timeSlots.Add(TimeSpan.FromHours(i));
            }

            if (id == null)
            {
                return NotFound();
            }


            var doctor = db.Doctors
                .Include(d => d.Hospital)
                .Include(d => d.Specialty)
                .Include(d => d.Hospital.Location)
                .FirstOrDefault(d => d.DoctorID == id);

            if (doctor == null)
            {
                return NotFound();
            }

            DateOnly dateOnly = new DateOnly(date.Year, date.Month, date.Day);

            var appointments = db.Appointments
                .Where(a => a.DoctorID == id && a.AppointmentDate == dateOnly).ToList();

            var viewModel = new BookViewModel
            {
                Doctor = doctor,
                Appointments = appointments,
                TimeSlots = timeSlots,
                SelectedDate = date
            };


            return View(viewModel);
        }


        [HttpGet]
        [Authorize]
        public ActionResult BookAppointment(int? id, DateTime date)
        {
            List<TimeSpan> timeSlots = new List<TimeSpan>();
            for (int i = 9; i < 35; i++)
            {
                // Assuming each integer represents an hour (e.g., 0 = 00:00, 1 = 01:00, etc.)
                timeSlots.Add(TimeSpan.FromHours(i));
            }

            if (id == null)
            {
                return NotFound();
            }


            var doctor = db.Doctors
                .Include(d => d.Hospital)
                .Include(d => d.Specialty)
                .Include(d => d.Hospital.Location)
                .FirstOrDefault(d => d.DoctorID == id);

            if (doctor == null)
            {
                return NotFound();
            }

            DateOnly dateOnly = new DateOnly(date.Year, date.Month, date.Day);

            var appointments = db.Appointments
                .Where(a => a.DoctorID == id && a.AppointmentDate == dateOnly).ToList();

            var viewModel2 = new BookViewModel
            {
                Doctor = doctor,
                Appointments = appointments,
                TimeSlots = timeSlots,
                SelectedDate = date

            };

            //ViewData["Date"] = date.ToShortDateString();
            //ModelState[nameof(Doctor.DoctorID)].RawValue = null;
            //ModelState[nameof(CheckBoxTestModel.Accept)].RawValue = null;
            //ModelState.SetModelValue(nameof(Doctor.DoctorID), new ValueProviderResult("", CultureInfo.InvariantCulture));
            //ValueProviderResult vpr = new ValueProviderResult("", System.Globalization.CultureInfo.CurrentCulture);

            // ModelState["id"].AttemptedValue = vpr.ToString();


            string jsonViewModel = JsonConvert.SerializeObject(viewModel2);

            // Return JSON result
            return Content(jsonViewModel, "application/json");
        }

        [HttpPost]
        public IActionResult BookTimeSlot(int id, DateTime date, int timeSlot)
        {
            if (User.Identity.IsAuthenticated)
            {
                // Get the username of the logged-in user
                string username = User.Identity.Name;

                // Retrieve user details using LINQ or any other method

                var userDetails = UserDb.Users.FirstOrDefault(u => u.UserName == username);

                if (userDetails == null)
                {
                    // User details found
                    return NotFound(); // Pass userDetails to the view
                }
                DateOnly dateOnly = new DateOnly(date.Year, date.Month, date.Day);


                var appointment = db.Appointments.FirstOrDefault(a => a.DoctorID == id &&
                                                                 a.AppointmentDate == dateOnly &&
                                                                 a.TimeSlot == timeSlot);

                if (appointment != null)
                {
                    // Update the appointment details
                    appointment.Status = true;
                    appointment.PatientName = userDetails.Name;
                    appointment.PatientEmail = userDetails.Email;
                    appointment.PatientPhoneNumber = userDetails.PhoneNumber;

                    // Save changes to the database
                    db.SaveChanges();

                }
            }
            return RedirectToAction("BookAppointment", new { id, date});
        }
    }
}
