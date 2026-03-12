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
                .Where(p => p.PatientID == patientId)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

            return View(prescriptions);
        }
    }
}