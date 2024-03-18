using LivDocApp.Data;
using LivDocApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace LivDocApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly DoctorsDbContext db;

        public HomeController(DoctorsDbContext context)
        {
            db = context;
        }
        public IActionResult Index()
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

            return View("index", results);
        }
        [HttpPost]
        public async Task<IActionResult> Book(int? id, DateTime date)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await db.Doctors
                .Include(d => d.Hospital)
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(d => d.DoctorID == id);

            if (doctor == null)
            {
                return NotFound();
            }

            var appointments = await db.Appointments
                .Where(a => a.DoctorID == id && a.AppointmentDate.ToShortDateString == date.ToShortDateString)
                .ToListAsync();

            var viewModel = new BookViewModel
            {
                Doctor = doctor,
                Appointments = appointments
            };

            ViewBag.Date = date.ToShortDateString();

            return View(viewModel);
        }

    }

}
