using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PracticeFlow.Data;
using PracticeFlow.Models;

namespace INF_SP.Controllers
{
    public class VitalsLogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VitalsLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int patientId)
        {
            // Check if user is logged in and is a doctor
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId) || role != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            // Get patient info
            var patient = await _context.Users.FindAsync(patientId);
            if (patient == null)
            {
                return NotFound();
            }

            // Get all vitals for this patient
            var vitals = await _context.VitalsLogs
                .Where(v => v.PatientID == patientId)
                .OrderByDescending(v => v.RecordedDate)
                .ToListAsync();

            ViewBag.PatientName = patient.FullName;
            ViewBag.PatientId = patientId;

            return View(vitals);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int patientId)
        {
            // Check if user is logged in and is a doctor
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId) || role != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            // Get patient info
            var patient = await _context.Users.FindAsync(patientId);
            if (patient == null)
            {
                return NotFound();
            }

            ViewBag.PatientName = patient.FullName;
            ViewBag.PatientId = patientId;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VitalsLog vitals)
        {
            // Check if user is logged in and is a doctor
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId) || role != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            vitals.RecordedDate = DateTime.Now;

            if (!ModelState.IsValid)
            {
                var patient = await _context.Users.FindAsync(vitals.PatientID);
                ViewBag.PatientName = patient?.FullName;
                ViewBag.PatientId = vitals.PatientID;
                return View(vitals);
            }

            _context.VitalsLogs.Add(vitals);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Vitals logged successfully!";
            return RedirectToAction("Index", new { patientId = vitals.PatientID });
        }

        // Patient views their own vitals
        public async Task<IActionResult> MyVitals()
        {
            // Check if user is logged in and is a patient
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId) || role != "Patient")
            {
                return RedirectToAction("Login", "Account");
            }

            var patientId = int.Parse(userId);

            // Get all vitals for this patient
            var vitals = await _context.VitalsLogs
                .Where(v => v.PatientID == patientId)
                .OrderByDescending(v => v.RecordedDate)
                .ToListAsync();

            ViewBag.PatientName = HttpContext.Session.GetString("FullName");

            return View(vitals);
        }
    }
}