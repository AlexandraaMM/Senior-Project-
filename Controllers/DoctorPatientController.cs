using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PracticeFlow.Data;

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

            var doctorId = int.Parse(userIdString);
            
            // Get all patients who have appointments with this doctor
            var patients = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId)
                .Select(a => a.Patient)
                .Distinct()
                .OrderBy(p => p.FullName)
                .ToListAsync();

            ViewBag.DoctorName = HttpContext.Session.GetString("FullName");
            return View(patients);
        }
    }
}