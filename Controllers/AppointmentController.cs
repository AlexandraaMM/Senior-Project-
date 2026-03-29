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

                // Get medical records created on the same day as the appointment
                var medicalRecords = await _context.MedicalRecords
                    .Where(r => r.DoctorId == doctorId)
                    .ToListAsync();

                
                var appointmentRecordMap = new Dictionary<int, int>();
                foreach (var appointment in appointments.Where(a => a.Status == "Completed"))
                {
                    var record = medicalRecords.FirstOrDefault(r => 
                        r.PatientId == appointment.PatientId &&
                        r.RecordDate.Date == appointment.AppointmentDate.Date);
                    
                    if (record != null)
                    {
                        appointmentRecordMap[appointment.AppointmentId] = record.RecordId;
                    }
                }

                ViewBag.AppointmentRecordMap = appointmentRecordMap;
                
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
        public async Task<IActionResult> Book(int DoctorId, string AppointmentDate, string AppointmentTime, string Reason)
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

                // Validate doctor selection
                if (DoctorId <= 0)
                {
                    ViewBag.ErrorMessage = "Please select a doctor";
                    ViewBag.Doctors = await _context.Users.Where(u => u.Role == "Doctor").ToListAsync();
                    return View();
                }

                DateTime appointmentDateTime;
                try
                {
                    var dateOnly = DateTime.Parse(AppointmentDate);
                    var timeParts = AppointmentTime.Split(':');
                    appointmentDateTime = new DateTime(
                        dateOnly.Year, dateOnly.Month, dateOnly.Day,
                        int.Parse(timeParts[0]), int.Parse(timeParts[1]), 0
                    );
                }
                catch
                {
                    ViewBag.ErrorMessage = "Invalid date or time format";
                    ViewBag.Doctors = await _context.Users.Where(u => u.Role == "Doctor").ToListAsync();
                    return View();
                }

                // Validate future date
                if (appointmentDateTime < DateTime.Now)
                {
                    ViewBag.ErrorMessage = "Cannot book appointments for past dates. Please select a future date and time.";
                    ViewBag.Doctors = await _context.Users.Where(u => u.Role == "Doctor").ToListAsync();
                    return View();
                }

                // Create appointment
                var appointment = new Appointment
                {
                    PatientId = int.Parse(userIdString),
                    DoctorId = DoctorId,
                    AppointmentDate = appointmentDateTime,
                    Reason = Reason ?? string.Empty,
                    Status = "Scheduled"
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Appointment booked successfully!";
                return RedirectToAction("MyAppointments");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Book POST: {ex.Message}");
                ViewBag.ErrorMessage = "An error occurred while booking the appointment. Please try again.";
                ViewBag.Doctors = await _context.Users.Where(u => u.Role == "Doctor").ToListAsync();
                return View();
            }
        }

        // Record visit from appointment
        public async Task<IActionResult> RecordVisit(int id)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userIdString) || role != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                TempData["Error"] = "Appointment not found";
                return RedirectToAction("DoctorAppointments");
            }

            // Mark appointment as completed
            appointment.Status = "Completed";
            await _context.SaveChangesAsync();

            return RedirectToAction("Create", "MedicalRecord", new { 
                patientId = appointment.PatientId, 
                reason = appointment.Reason 
            });
        }

        // Cancel Appointment
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userIdString) || role != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
            {
                TempData["Error"] = "Appointment not found";
                return RedirectToAction("DoctorAppointments");
            }

            appointment.Status = "Cancelled";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment cancelled successfully";
            return RedirectToAction("DoctorAppointments");
        }
    }
}