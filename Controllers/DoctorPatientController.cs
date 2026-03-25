using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PracticeFlow.Data;
using PracticeFlow.Models;

namespace INF_SP.Controllers
{
    public class DoctorPatientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorPatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ViewPatients()
        {
            // Check if user is logged in and is a doctor
            var userIdString = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userIdString) || role != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            // Get all patients in the system
            var patients = await _context.Users
                .Where(u => u.Role == "Patient")
                .OrderBy(p => p.FullName)
                .ToListAsync();

            ViewBag.DoctorName = HttpContext.Session.GetString("FullName");
            return View(patients);
        }

        // View patient details
        public async Task<IActionResult> ViewDetails(int id)
        {
            // Check if user is logged in and is a doctor
            var userIdString = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userIdString) || role != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            var patient = await _context.Users.FindAsync(id);
            
            if (patient == null || patient.Role != "Patient")
            {
                TempData["Error"] = "Patient not found";
                return RedirectToAction("ViewPatients");
            }

            return View(patient);
        }

        // View patient medical history
        public async Task<IActionResult> MedicalHistory(int id)
        {
            // Check if user is logged in and is a doctor
            var userIdString = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userIdString) || role != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            var patient = await _context.Users.FindAsync(id);
            
            if (patient == null || patient.Role != "Patient")
            {
                TempData["Error"] = "Patient not found";
                return RedirectToAction("ViewPatients");
            }

            // Get all medical records for this patient
            var records = await _context.MedicalRecords
                .Include(r => r.Doctor)
                .Where(r => r.PatientId == id)
                .OrderByDescending(r => r.RecordDate)
                .ToListAsync();

            ViewBag.PatientName = patient.FullName;
            return View(records);
        }

        public async Task<IActionResult> LogVitals(int id)
        {
            // Check if user is logged in and is a doctor
            var userIdString = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userIdString) || role != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            var patient = await _context.Users.FindAsync(id);
            
            if (patient == null || patient.Role != "Patient")
            {
                TempData["Error"] = "Patient not found";
                return RedirectToAction("ViewPatients");
            }

            ViewBag.PatientName = patient.FullName;
            ViewBag.PatientID = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogVitals(VitalsLog vitals)
        {
            // Check if user is logged in and is a doctor
            var userIdString = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userIdString) || role != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            vitals.RecordedDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.VitalsLogs.Add(vitals);
                await _context.SaveChangesAsync();
                
                return RedirectToAction("ViewVitals", new { id = vitals.PatientID });
            }

            var patient = await _context.Users.FindAsync(vitals.PatientID);
            ViewBag.PatientName = patient?.FullName;
            ViewBag.PatientID = vitals.PatientID;
            return View(vitals);
        }

        // View patient vitals history
        public async Task<IActionResult> ViewVitals(int id)
        {
            // Check if user is logged in and is a doctor
            var userIdString = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userIdString) || role != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            var patient = await _context.Users.FindAsync(id);
            
            if (patient == null || patient.Role != "Patient")
            {
                TempData["Error"] = "Patient not found";
                return RedirectToAction("ViewPatients");
            }

            // Get all vitals for this patient
            var vitals = await _context.VitalsLogs
                .Where(v => v.PatientID == id)
                .OrderByDescending(v => v.RecordedDate)
                .ToListAsync();

            ViewBag.PatientName = patient.FullName;
            return View(vitals);
        }
    }
}