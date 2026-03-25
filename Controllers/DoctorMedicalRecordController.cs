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

            return View(record);
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
            ViewBag.PatientName = record.Patient.FullName;
            ViewBag.PatientID = record.PatientId;
            return View();
        }

        // Add prescription
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
                return RedirectToAction("ViewRecords");
            }

            var record = await _context.MedicalRecords
                .Include(r => r.Patient)
                .FirstOrDefaultAsync(r => r.RecordId == prescription.RecordId);
            
            ViewBag.RecordId = prescription.RecordId;
            ViewBag.PatientName = record?.Patient.FullName;
            ViewBag.PatientID = record?.PatientId;
            return View(prescription);
        }
    }
}