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
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.RecordDate)
                .ToListAsync();

            return View(records);
        }

        public IActionResult Create()
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

            // Get doctor's patients from appointments
            var doctorId = int.Parse(HttpContext.Session.GetString("UserId"));
            var patients = _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .Select(a => a.PatientId)
                .Distinct()
                .ToList();

            var patientList = _context.Users
                .Where(u => patients.Contains(u.UserID))
                .Select(u => new SelectListItem
                {
                    Value = u.UserID.ToString(),
                    Text = u.FullName
                })
                .ToList();

            ViewBag.Patients = patientList;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MedicalRecord record)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("UserId") == null)
            {
                Console.WriteLine("❌ User not logged in");
                return RedirectToAction("Login", "Account");
            }

            // Check if user is a doctor
            var role = HttpContext.Session.GetString("Role");
            if (role != "Doctor")
            {
                Console.WriteLine("❌ User is not a doctor: " + role);
                return RedirectToAction("Index", "Home");
            }

            record.DoctorId = int.Parse(HttpContext.Session.GetString("UserId"));
            record.RecordDate = DateTime.Now;

            Console.WriteLine("🔍 Submitted Data:");
            Console.WriteLine("   PatientId: " + record.PatientId);
            Console.WriteLine("   DoctorId: " + record.DoctorId);
            Console.WriteLine("   ReasonForVisit: " + record.ReasonForVisit);
            Console.WriteLine("   Diagnosis: " + record.Diagnosis);
            Console.WriteLine("   ModelState.IsValid: " + ModelState.IsValid);

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ Validation errors:");
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    if (errors.Count > 0)
                    {
                        Console.WriteLine($"   Field '{key}':");
                        foreach (var error in errors)
                        {
                            Console.WriteLine($"      - {error.ErrorMessage}");
                        }
                    }
                }
                
                // Reload patient list
                var doctorId = int.Parse(HttpContext.Session.GetString("UserId"));
                var patients = _context.Appointments
                    .Where(a => a.DoctorId == doctorId)
                    .Select(a => a.PatientId)
                    .Distinct()
                    .ToList();

                var patientList = _context.Users
                    .Where(u => patients.Contains(u.UserID))
                    .Select(u => new SelectListItem
                    {
                        Value = u.UserID.ToString(),
                        Text = u.FullName
                    })
                    .ToList();

                ViewBag.Patients = patientList;
                return View(record);
            }

            Console.WriteLine("✅ Validation passed! Saving to database...");
            _context.MedicalRecords.Add(record);
            _context.SaveChanges();
            Console.WriteLine("✅ Record saved! RecordId: " + record.RecordId);

            TempData["Success"] = "Medical record created successfully!";
            return RedirectToAction("DoctorDashboard", "Home");
        }
    }
}