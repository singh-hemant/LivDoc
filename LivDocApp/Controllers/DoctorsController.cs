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

namespace LivDocApp.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly DoctorsDbContext _context;

        public DoctorsController(DoctorsDbContext context)
        {
            _context = context;
        }

        // GET: Doctors
        public async Task<IActionResult> Index()
        {
            var doctorsDbContext = _context.Doctors.Include(d => d.Hospital).Include(d => d.Specialty);
            return View(await doctorsDbContext.ToListAsync());
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
            ViewData["HospitalID"] = new SelectList(_context.Set<Hospital>(), "HospitalID", "Name");
            ViewData["SpecialtyID"] = new SelectList(_context.Set<Specialty>(), "SpecialtyId", "Name");
            return View();
        }

        // POST: Doctors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DoctorID,Name,Experience,SpecialtyID,HospitalID,Email,PhoneNumber,DocImgURL")] Doctor doctor, IFormFile imgFile)
        {
            if (ModelState.IsValid)
            {
                Console.WriteLine("Model is valid");
                if (imgFile != null && imgFile.Length > 0)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "/DoctorsImg/", imgFile.FileName);
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
        public async Task<IActionResult> Edit(int id, [Bind("DoctorID,Name,Experience,SpecialtyID,HospitalID,Email,PhoneNumber,DocImgURL")] Doctor doctor, IFormFile imgFile)
        {
            Console.WriteLine(" Edit1");
            if (id != doctor.DoctorID)
            {
                Console.WriteLine(" Edit2");

                return NotFound();
            }

            if (ModelState.IsValid)
            {
                Console.WriteLine(" Edit3");
                if (imgFile != null && imgFile.Length > 0)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "/DoctorsImg/", imgFile.FileName);
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
