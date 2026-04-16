using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PracticeFlow.Data;
using PracticeFlow.Models;

namespace INF_SP.Controllers
{
    public class DoctorMedicalRecordController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorMedicalRecordController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ViewRecords()
        {
            // Check if user is logged in and is a doctor
            var userIdString = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userIdString) || role != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            var doctorId = int.Parse(userIdString);
            
            // Get all medical records created by this doctor
            var records = await _context.MedicalRecords
                .Include(m => m.Patient)
                .Where(m => m.DoctorId == doctorId)
                .OrderByDescending(m => m.RecordDate)
                .ToListAsync();

            ViewBag.DoctorName = HttpContext.Session.GetString("FullName");
            return View(records);
        }

        // View medical record details
        public async Task<IActionResult> ViewDetails(int id)
        {
            // Check if user is logged in and is a doctor
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId) || role != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            var record = await _context.MedicalRecords
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .FirstOrDefaultAsync(r => r.RecordId == id);

            if (record == null)
            {
                TempData["Error"] = "Medical record not found";
                return RedirectToAction("ViewRecords");
            }

            // Load prescriptions for this record
            ViewBag.Prescriptions = await _context.Prescriptions
                .Where(p => p.RecordId == id)
                .ToListAsync();

            // Load patient's vitals history 
            ViewBag.VitalsHistory = await _context.VitalsLogs
                .Where(v => v.PatientID == record.PatientId)
                .OrderByDescending(v => v.RecordedDate)
                .Take(10)
                .ToListAsync();

            return View(record);
        }

        // View patient's complete medical history
        public async Task<IActionResult> PatientHistory(int patientId)
        {
            // Check if user is logged in and is a doctor
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId) || role != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            var patient = await _context.Users.FindAsync(patientId);
            if (patient == null)
            {
                return NotFound();
            }

            // Get all medical records for this patient
            var records = await _context.MedicalRecords
                .Include(r => r.Doctor)
                .Include(r => r.Prescriptions)
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.RecordDate)
                .ToListAsync();

            ViewBag.PatientName = patient.FullName;
            ViewBag.PatientId = patientId;

            return View(records);
        }

        // Add prescription to medical record
        public async Task<IActionResult> AddPrescription(int id)
        {
            // Check if user is logged in and is a doctor
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId) || role != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            var record = await _context.MedicalRecords
                .Include(r => r.Patient)
                .FirstOrDefaultAsync(r => r.RecordId == id);
            
            if (record == null)
            {
                TempData["Error"] = "Medical record not found";
                return RedirectToAction("ViewRecords");
            }

            ViewBag.RecordId = id;
            ViewBag.PatientName = record.Patient!.FullName;
            ViewBag.PatientID = record.PatientId;
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPrescription(Prescription prescription)
        {
            // Check if user is logged in and is a doctor
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId) || role != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            prescription.CreatedDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Prescription added successfully!";
                return RedirectToAction("ViewDetails", new { id = prescription.RecordId });
            }

            var record = await _context.MedicalRecords
                .Include(r => r.Patient)
                .FirstOrDefaultAsync(r => r.RecordId == prescription.RecordId);
            
            ViewBag.RecordId = prescription.RecordId;
            ViewBag.PatientName = record?.Patient!.FullName;
            ViewBag.PatientID = record?.PatientId;
            return View(prescription);
        }
    }
}