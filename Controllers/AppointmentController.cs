using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PracticeFlow.Data;
using PracticeFlow.Models;
using Microsoft.AspNetCore.Http; 

namespace INF_SP.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Patients view their appointments
        public async Task<IActionResult> MyAppointments()
        {
            try
            {
                // Check if user is logged in
                var userIdString = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdString))
                {
                    return RedirectToAction("Login", "Account");
                }

                var patientId = int.Parse(userIdString);
                var appointments = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Where(a => a.PatientId == patientId)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ToListAsync();
                
                return View(appointments);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error in MyAppointments: {ex.Message}");
                return View(new List<Appointment>());
            }
        }

        // Doctors view appointments with their patients
        public async Task<IActionResult> DoctorAppointments()
        {
            try
            {
                // Check if user is logged in and is a doctor
                var userIdString = HttpContext.Session.GetString("UserId");
                var role = HttpContext.Session.GetString("Role");
                
                if (string.IsNullOrEmpty(userIdString) || role != "Doctor")
                {
                    return RedirectToAction("Login", "Account");
                }

                var doctorId = int.Parse(userIdString);
                var appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Where(a => a.DoctorId == doctorId)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ToListAsync();
                
                return View(appointments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DoctorAppointments: {ex.Message}");
                return View(new List<Appointment>());
            }
        }

        
        [HttpGet]
        public async Task<IActionResult> Book()
        {
            try
            {
                // Check if user is logged in and is a patient
                var userIdString = HttpContext.Session.GetString("UserId");
                var role = HttpContext.Session.GetString("Role");
                
                if (string.IsNullOrEmpty(userIdString) || role != "Patient")
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get list of doctors
                var doctors = await _context.Users
                    .Where(u => u.Role == "Doctor")
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                if (!doctors.Any())
                {
                    ViewBag.ErrorMessage = "No doctors available at this time.";
                }

                ViewBag.Doctors = doctors;
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Book GET: {ex.Message}");
                return RedirectToAction("MyAppointments");
            }
        }

        // Book new appointment
        [HttpPost]
        public async Task<IActionResult> Book(Appointment appointment)
        {
            try
            {
                // Check if user is logged in
                var userIdString = HttpContext.Session.GetString("UserId");
                var role = HttpContext.Session.GetString("Role");
                
                if (string.IsNullOrEmpty(userIdString) || role != "Patient")
                {
                    return RedirectToAction("Login", "Account");
                }

                
                if (!ModelState.IsValid)
                {
                    ViewBag.Doctors = await _context.Users
                        .Where(u => u.Role == "Doctor")
                        .OrderBy(u => u.FullName)
                        .ToListAsync();
                    return View(appointment);
                }

                
                if (appointment.DoctorId <= 0)
                {
                    ModelState.AddModelError("DoctorId", "Please select a doctor");
                    ViewBag.Doctors = await _context.Users
                        .Where(u => u.Role == "Doctor")
                        .OrderBy(u => u.FullName)
                        .ToListAsync();
                    return View(appointment);
                }

                
                if (appointment.AppointmentDate == default || appointment.AppointmentDate < DateTime.Now)
                {
                    ModelState.AddModelError("AppointmentDate", "Please select a valid future date and time");
                    ViewBag.Doctors = await _context.Users
                        .Where(u => u.Role == "Doctor")
                        .OrderBy(u => u.FullName)
                        .ToListAsync();
                    return View(appointment);
                }

                
                appointment.PatientId = int.Parse(userIdString);
                appointment.Status = "Scheduled";
                
                appointment.Reason = appointment.Reason ?? string.Empty;

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Appointment booked successfully!";
                return RedirectToAction("MyAppointments");
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
                ModelState.AddModelError("", "Unable to save appointment. Please try again.");
                
                ViewBag.Doctors = await _context.Users
                    .Where(u => u.Role == "Doctor")
                    .OrderBy(u => u.FullName)
                    .ToListAsync();
                
                return View(appointment);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Book POST: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                
                ViewBag.Doctors = await _context.Users
                    .Where(u => u.Role == "Doctor")
                    .OrderBy(u => u.FullName)
                    .ToListAsync();
                
                return View(appointment);
            }
        }
    }
}