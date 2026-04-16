using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PracticeFlow.Data;
using PracticeFlow.Models;

namespace INF_SP.Controllers
{
    public class PrescriptionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrescriptionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Patient views their prescriptions
        public async Task<IActionResult> MyPrescriptions()
        {
            // Check if user is logged in and is a patient
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId) || role != "Patient")
            {
                return RedirectToAction("Login", "Account");
            }

            var patientId = int.Parse(userId);
            var prescriptions = await _context.Prescriptions
                .Include(p => p.MedicalRecord)
                    .ThenInclude(m => m!.Doctor)  
                .Where(p => p.PatientID == patientId)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

            return View(prescriptions);
        }

        // Doctor creates prescription for a medical record
        [HttpGet]
        public async Task<IActionResult> Create(int recordId)
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
                .FirstOrDefaultAsync(r => r.RecordId == recordId);
            
            if (record == null)
            {
                return NotFound();
            }

            ViewBag.RecordId = recordId;
            ViewBag.PatientName = record.Patient!.FullName;
            ViewBag.RecordDate = record.RecordDate.ToString("MMM dd, yyyy");
            ViewBag.ReasonForVisit = record.ReasonForVisit;
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Prescription prescription)
        {
            // Check if user is logged in and is a doctor
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId) || role != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                // Reload record info
                var record = await _context.MedicalRecords
                    .Include(r => r.Patient)
                    .FirstOrDefaultAsync(r => r.RecordId == prescription.RecordId);
                
                if (record != null)
                {
                    ViewBag.RecordId = prescription.RecordId;
                    ViewBag.PatientName = record.Patient!.FullName;
                    ViewBag.RecordDate = record.RecordDate.ToString("MMM dd, yyyy");
                    ViewBag.ReasonForVisit = record.ReasonForVisit;
                }
                
                return View(prescription);
            }

            prescription.CreatedDate = DateTime.Now;
            
            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Prescription added successfully!";
            return RedirectToAction("ViewDetails", "DoctorMedicalRecord", new { id = prescription.RecordId });
        }
    }
}