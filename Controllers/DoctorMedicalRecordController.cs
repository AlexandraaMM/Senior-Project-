using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PracticeFlow.Data;

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
    }
}