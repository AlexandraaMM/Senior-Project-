using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PracticeFlow.Data;
using PracticeFlow.Models;
using Microsoft.AspNetCore.Http;

namespace INF_SP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetString("UserId");
            
            if (string.IsNullOrEmpty(userId))
            {
                // If not logged in go to login page
                return RedirectToAction("Login", "Account");
            }

            
            var role = HttpContext.Session.GetString("Role");
            
            switch (role)
            {
                case "Admin":
                    return RedirectToAction("AdminDashboard");
                case "Doctor":
                    return RedirectToAction("DoctorDashboard");
                case "Patient":
                    return RedirectToAction("PatientDashboard");
                default:
                    return View();
            }
        }
        
        public IActionResult AdminDashboard()
        {
            
            {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId) || role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            return RedirectToAction("ViewUsers", "Admin");
            }
        }

        public IActionResult DoctorDashboard()
        {
            
            var role = HttpContext.Session.GetString("Role");
            if (role != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.FullName = HttpContext.Session.GetString("FullName");
            ViewBag.Role = role;
            
            return View();
        }

        
        public async Task<IActionResult> PatientDashboard()
       
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId) || role != "Patient")
            {
                return RedirectToAction("Login", "Account");
            }

            var patientId = int.Parse(userId);

            ViewBag.UpcomingCount = await _context.Appointments
                .Where(a => a.PatientId == patientId && 
                            a.AppointmentDate >= DateTime.Now &&
                            a.Status == "Scheduled")
                .CountAsync();

            ViewBag.RecordsCount = await _context.MedicalRecords
                .Where(r => r.PatientId == patientId)
                .CountAsync();

            ViewBag.PrescriptionsCount = await _context.Prescriptions
                .Where(p => p.PatientID == patientId)
                .CountAsync();

            ViewBag.FullName = HttpContext.Session.GetString("FullName");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}