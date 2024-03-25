using LivDocApp.Data;
using LivDocApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Twilio.Types;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Azure.Communication.Sms;


namespace LivDocApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly DoctorsDbContext db;
        private readonly ApplicationDbContext UserDb;
        private readonly IConfiguration _configuration;

        public HomeController(DoctorsDbContext context, ApplicationDbContext context1, IConfiguration configuration)
        {
            db = context;
            UserDb = context1;
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }

       
        public IActionResult Services()
        {
            return View();
        }
      

        public IActionResult PageSearch()
        {
            return View();
        }


        [Authorize]
        [HttpGet]
        public ActionResult Search(string searchTerm)
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

            string jsonViewModel = JsonConvert.SerializeObject(results);

            // Return JSON result
            return Content(jsonViewModel, "application/json");
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
        public IActionResult BookTimeSlot(int id, DateTime date, int timeSlot, string tsText)
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

                    //send booking confirmation email
                    var doctor = db.Doctors
                .Include(d => d.Hospital)
                .Include(d => d.Specialty)
                .Include(d => d.Hospital.Location)
                .FirstOrDefault(d => d.DoctorID == id);

                    var pName = userDetails.Name;
                    var pEmail = userDetails.Email;
                    var dname = doctor.Name;
                    var dLoc = doctor.Hospital.Location.City;
                    var dHos = doctor.Hospital.Name;
                    var aptDate = appointment.AppointmentDate.ToShortDateString();



                    var subject = "Booking Confirmation";
                    var msgBody = $"<h4>Hello {pName},</h4>" +
                                    $"<p>Your booking is <b>confirmed.<b/></p>" +
                                    $"<h4>Your Booking Details: </h4>" +
                                    $"<table border='1' width='800px' height='200px'><tr><th>Doctor Name</th><th>Location</th><th>Hospital</th><th>Date</th><th>Time Slot</th></tr>"+
                    $"<tr><td>{dname}</td><td>{dLoc}</td><td>{dHos}</td><td>{aptDate}</td><td>{tsText}</td></tr></table>";

                    var msgBody2 = $"Hello {pName}\n" +
                                    $"Your booking is confirmed.\n" +
                                    $"Your Booking Details: >" +
                                    $"Doctor Name:{dname}\n Location: {dLoc}\nHospital: {dHos}\nDate: {aptDate}\nTime Slot: {tsText}";

                    SendEmail(subject, pEmail, msgBody);

                    SendSms(userDetails.PhoneNumber, msgBody2);

                }
            }
            return RedirectToAction("BookAppointment", new { id, date});
        }

        private void SendEmail(string subject, string recipientEmail, string msgBody)
        {
            // Configure SMTP client
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
            smtpClient.Port = 587; // Update with the appropriate port
            smtpClient.Credentials = new NetworkCredential("hemantsinghritesh@gmail.com", "grbgffyuzqengpsa");
            smtpClient.EnableSsl = true;

            // Create MailMessage
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("sin-hem@outlook.com");
            mailMessage.To.Add(recipientEmail);
            mailMessage.Subject = subject;
            mailMessage.Body = msgBody;
            mailMessage.IsBodyHtml = true;

            try
            {
                // Send email
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                // Handle exception
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }


        private void SendSms(string to, string message)
        {
            
            var accountSid =  _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            var twilioPhoneNumber = _configuration["Twilio:PhoneNumber"];



            TwilioClient.Init(accountSid, authToken);

            var toPhoneNumber = new PhoneNumber(to);
            var messageOptions = new CreateMessageOptions(toPhoneNumber)
            {
                From = new PhoneNumber(twilioPhoneNumber),
                Body = message
            };
            try
            {
                MessageResource.Create(messageOptions);

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           

            /*
            string connectionString = "endpoint=https://livdoc-sms.india.communication.azure.com/;accesskey=hromRfeuZsRpRy7p7UrBGzE9yQwl7oacFqlu0w0mbE1CSaONUFr0bnvsaGNoJxU4z8X5wTlCUe7EC1KvMWO9sA==";

            // Replace these with your actual phone numbers
            string fromPhoneNumber = "+91831805397";
            string toPhoneNumber = to;

            // Message content
            string msg = message;

            // Initialize the SmsClient
            var client = new SmsClient(connectionString);

            // Send the SMS message
            SmsSendResult response = client.Send(fromPhoneNumber, toPhoneNumber, message);

            // Handle the response
            if (response.Successful)
            {
                Console.WriteLine("SMS message sent successfully!");
            }
            else
            {
                Console.WriteLine($"Failed to send SMS message. Error: {response.ErrorMessage}");
            }
            */

        }
    

        [Authorize]
        public IActionResult BookinList()
        {
            List<TimeSpan> timeSlots = new List<TimeSpan>();
            for (int i = 9; i < 35; i++)
            {
                // Assuming each integer represents an hour (e.g., 0 = 00:00, 1 = 01:00, etc.)
                timeSlots.Add(TimeSpan.FromHours(i));
            }

            // Get the username of the logged-in user
            string username = User.Identity.Name;

            var userDetails = UserDb.Users.FirstOrDefault(u => u.UserName == username);
            var appointments =  db.Appointments.Where(a => a.PatientEmail == userDetails.Email && a.Status == true).ToList();
            var doctors = (from appointment in appointments
                           join doctor in db.Doctors.Include(d => d.Hospital).Include(d => d.Specialty).Include(d => d.Hospital.Location) on appointment.DoctorID equals doctor.DoctorID
                           select doctor).Distinct().ToList();


            var viewModel = new BookingListModel
            {
                Appointments = appointments,
                Doctors = doctors,
                TimeSlots = timeSlots
            };
            return View(viewModel);
        }

        public IActionResult CancelBooking(int? id, string timeSlot)
        {
            if (id == null)
            {
                return BadRequest(); // If id is null, return a bad request response
            }

            var appointment = db.Appointments.FirstOrDefault(a => a.AppointmentID == id);

            if (appointment == null)
            {
                return NotFound(); // If appointment with specified id is not found, return a not found response
            }

            // Get the username of the logged-in user
            string username = User.Identity.Name;

            // Retrieve user details using LINQ or any other method

            var userDetails = UserDb.Users.FirstOrDefault(u => u.UserName == username);
            var pName = userDetails.Name;
            var pPN = userDetails.PhoneNumber;
            var pEmail = userDetails.Email;


            // Remove the appointment from the database
            appointment.Status = false;
            appointment.PatientEmail = null;
            appointment.PatientName = null;
            appointment.PatientPhoneNumber = null;
            db.SaveChanges();
            // Send Cancellation email
            var subject = "Appointment cancellation information";
            var msgBody = $"<h4>Hello {pName},</h4>" +
                            $"<h5>Your Appointment on {appointment.AppointmentDate} at {timeSlot} has been <b>Cancelled.<b/></h5>";
                     

            var msgBody2 = $"Hello {pName},\n" +
                            $"Your Appointment on {appointment.AppointmentDate} at {timeSlot} has been <b>Cancelled.";


            SendEmail(subject, pEmail, msgBody);

            SendSms(userDetails.PhoneNumber, msgBody2);

            return RedirectToAction("BookinList"); // Redirect to the booking list page after cancellation
        }
    }
}
