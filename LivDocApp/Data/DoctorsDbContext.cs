using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LivDocApp.Models;

namespace LivDocApp.Data
{
    public class DoctorsDbContext : DbContext
    {
        public DoctorsDbContext (DbContextOptions<DoctorsDbContext> options)
            : base(options)
        {
        }

        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
    }
}