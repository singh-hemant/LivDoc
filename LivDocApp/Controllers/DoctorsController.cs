using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LivDocApp.Data;
using LivDocApp.Models;
using System.Drawing.Printing;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace LivDocApp.Controllers
{
    [Authorize]
    public class DoctorsController : Controller
    {
        private readonly DoctorsDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DoctorsController(DoctorsDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }


        // GET: Doctors
        public async Task<IActionResult> Index()
        {
            //var doctorsDbContext = _context.Doctors.Include(d => d.Hospital).Include(d => d.Specialty);
            //return View(await doctorsDbContext.ToListAsync());

            var doctors = await _context.Doctors
            .Include(d => d.Hospital)
            .Include(d => d.Hospital.Location)
            .Include(d => d.Specialty)
            .ToListAsync();

            //return Json(doctors);

            var jsonContent = JsonConvert.SerializeObject(doctors);

            // Pass JSON data to the view
            ViewData["Doctors"] = jsonContent;
            return View();
        }

            // GET: Doctors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.Hospital)
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(m => m.DoctorID == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // GET: Doctors/Create
        public IActionResult Create()
        {
            ViewData["HospitalID"] = new SelectList(_context.Hospitals, "HospitalID", "Name");
            ViewData["SpecialtyID"] = new SelectList(_context.Set<Specialty>(), "SpecialtyId", "Name");
            return View();
        }

        // POST: Doctors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DoctorID,Name,Experience,SpecialtyID,HospitalID,Email,PhoneNumber")] Doctor doctor, [FromForm]  IFormFile imgFile)
        {
            ModelState.Remove("DocImgURL");
            ModelState.Remove("imgFile");
            ModelState.Remove("Hospital");
            ModelState.Remove("Specialty");
            if (ModelState.IsValid)
            {
                Console.WriteLine("Model is valid");
                if (imgFile != null && imgFile.Length > 0)
                {
                    var webRootPath = _env.WebRootPath;
                    var uploadsPath = Path.Combine(webRootPath, "DoctorsImg");

                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var filePath = Path.Combine(uploadsPath, imgFile.FileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imgFile.CopyToAsync(fileStream);
                    }
                    doctor.DocImgURL = "/DoctorsImg/" + imgFile.FileName;
                }
                _context.Add(doctor);
               // _context.SaveChanges();
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["HospitalID"] = new SelectList(_context.Set<Hospital>(), "HospitalID", "Name", doctor.HospitalID);
            ViewData["SpecialtyID"] = new SelectList(_context.Set<Specialty>(), "SpecialtyId", "Name", doctor.SpecialtyID);
            return View(doctor);
        }

        // GET: Doctors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }
            ViewData["HospitalID"] = new SelectList(_context.Set<Hospital>(), "HospitalID", "Name", doctor.HospitalID);
            ViewData["SpecialtyID"] = new SelectList(_context.Set<Specialty>(), "SpecialtyId", "Name", doctor.SpecialtyID);
            return View(doctor);
        }

        // POST: Doctors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DoctorID,Name,Experience,SpecialtyID,HospitalID,Email,PhoneNumber")] Doctor doctor, [FromForm] IFormFile imgFile)
        {          
            if (id != doctor.DoctorID)
            {
                return NotFound();
            }
            ModelState.Remove("DocImgURL");
            ModelState.Remove("imgFile");
            ModelState.Remove("Hospital");
            ModelState.Remove("Specialty");

            if (ModelState.IsValid)
            {
     
                if (imgFile != null && imgFile.Length > 0)
                {
                    var webRootPath = _env.WebRootPath;
                    var uploadsPath = Path.Combine(webRootPath, "DoctorsImg");

                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var filePath = Path.Combine(uploadsPath, imgFile.FileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imgFile.CopyToAsync(fileStream);
                    }
                    doctor.DocImgURL = "/DoctorsImg/" + imgFile.FileName;
                }

                try
                {
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.DoctorID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["HospitalID"] = new SelectList(_context.Set<Hospital>(), "HospitalID", "Name", doctor.HospitalID);
            ViewData["SpecialtyID"] = new SelectList(_context.Set<Specialty>(), "SpecialtyId", "Name", doctor.SpecialtyID);
            return View(doctor);
        }

        // GET: Doctors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.Hospital)
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(m => m.DoctorID == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: Doctors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.DoctorID == id);
        }
    }
}
