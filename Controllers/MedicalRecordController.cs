using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PracticeFlow.Data;
using PracticeFlow.Models;
using Microsoft.AspNetCore.Http;

namespace INF_SP.Controllers
{
    public class MedicalRecordController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MedicalRecordController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> MyRecords()
        {
            // Check if user is logged in and is a patient
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId) || role != "Patient")
            {
                return RedirectToAction("Login", "Account");
            }

            var patientId = int.Parse(userId);
            var records = await _context.MedicalRecords
                .Include(r => r.Doctor)
                .Include(r => r.Prescriptions)
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.RecordDate)
                .ToListAsync();

            return View(records);
        }

        public async Task<IActionResult> Create(int? patientId, string? reason, int? appointmentId)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if user is a doctor
            var role = HttpContext.Session.GetString("Role");
            if (role != "Doctor")
            {
                return RedirectToAction("Index", "Home");
            }

            // Get all patients in the system
            var patientList = await _context.Users
                .Where(u => u.Role == "Patient")
                .OrderBy(u => u.FullName)
                .Select(u => new SelectListItem
                {
                    Value = u.UserID.ToString(),
                    Text = u.FullName
                })
                .ToListAsync();

            ViewBag.Patients = patientList;

            if (patientId.HasValue)
            {
                var patient = await _context.Users.FindAsync(patientId.Value);
                ViewBag.PreSelectedPatient = patient;
                ViewBag.PreFilledReason = reason;
                ViewBag.IsFromAppointment = true;
                ViewBag.AppointmentId = appointmentId;
            }
            else
            {
                ViewBag.IsFromAppointment = false;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicalRecord record, Prescription Prescription, string WalkInPatientName)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if user is a doctor
            var role = HttpContext.Session.GetString("Role");
            if (role != "Doctor")
            {
                return RedirectToAction("Index", "Home");
            }

            // Handle walk-in patient 
            if (!string.IsNullOrWhiteSpace(WalkInPatientName))
            {
                // Check if a patient with this name already exists
                var existingPatient = await _context.Users
                    .FirstOrDefaultAsync(u => u.Role == "Patient" && u.FullName.ToLower() == WalkInPatientName.ToLower());
                
                if (existingPatient != null)
                {
                    // Patient already exists - use their account
                    record.PatientId = existingPatient.UserID;
                }
                else
                {
                    // Create a new patient account
                    var walkInPatient = new User
                    {
                        FullName = WalkInPatientName,
                        Username = "walkin_" + DateTime.Now.Ticks, 
                        Email = $"walkin_{DateTime.Now.Ticks}@temp.local", 
                        Role = "Patient",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("ChangeMe123"), 
                        DateOfBirth = null
                    };

                    _context.Users.Add(walkInPatient);
                    await _context.SaveChangesAsync();

                    record.PatientId = walkInPatient.UserID;
                }
            }

            record.DoctorId = int.Parse(HttpContext.Session.GetString("UserId")!);
            record.RecordDate = DateTime.Now;

            // Remove prescription validation if not provided
            if (string.IsNullOrWhiteSpace(Prescription.MedicationName))
            {
                ModelState.Remove("Prescription.MedicationName");
                ModelState.Remove("Prescription.Dosage");
                ModelState.Remove("Prescription.Instructions");
            }

            // Remove PatientId validation error for walk-ins
            if (!string.IsNullOrWhiteSpace(WalkInPatientName))
            {
                ModelState.Remove("PatientId");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.SavedReasonForVisit = record.ReasonForVisit;
                ViewBag.SavedDiagnosis = record.Diagnosis;
                ViewBag.SavedTreatment = record.Treatment;
                ViewBag.SavedNotes = record.Notes;
                ViewBag.SavedWalkInName = WalkInPatientName;
                ViewBag.SavedMedicationName = Prescription.MedicationName;
                ViewBag.SavedDosage = Prescription.Dosage;
                ViewBag.SavedInstructions = Prescription.Instructions;
                ViewBag.SavedStartDate = Prescription.StartDate.ToString("yyyy-MM-dd");
                ViewBag.SavedEndDate = Prescription.EndDate?.ToString("yyyy-MM-dd");
                
                var patientList = await _context.Users
                    .Where(u => u.Role == "Patient")
                    .OrderBy(u => u.FullName)
                    .Select(u => new SelectListItem
                    {
                        Value = u.UserID.ToString(),
                        Text = u.FullName
                    })
                    .ToListAsync();

                ViewBag.Patients = patientList;
                return View(record);
            }

            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            if (Request.Form.ContainsKey("AppointmentId") && !string.IsNullOrEmpty(Request.Form["AppointmentId"]))
            {
                var appointmentId = int.Parse(Request.Form["AppointmentId"]!);
                
                // Link the medical record to the appointment
                record.AppointmentId = appointmentId;
                await _context.SaveChangesAsync();
                
                // Mark appointment as completed
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment != null)
                {
                    appointment.Status = "Completed";
                    await _context.SaveChangesAsync();
                }
            }

            if (!string.IsNullOrWhiteSpace(Prescription.MedicationName))
            {
                Prescription.RecordId = record.RecordId;
                Prescription.PatientID = record.PatientId;
                Prescription.CreatedDate = DateTime.Now;
                
                _context.Prescriptions.Add(Prescription);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Medical record created successfully!";
            return RedirectToAction("ViewRecords", "DoctorMedicalRecord");
        }

        public async Task<IActionResult> ViewPrescription(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var record = await _context.MedicalRecords
                .Include(m => m.Doctor)
                .Include(m => m.Patient)
                .Include(m => m.Prescriptions)
                .FirstOrDefaultAsync(m => m.RecordId == id && m.PatientId == int.Parse(userId));

            if (record == null)
            {
                return NotFound();
            }

            return View(record);
        }
    }
}