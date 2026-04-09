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

        // This is the main page and it redirects to the right dashboard
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
            
            // Send them to their specific dashboard
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
            var role = HttpContext.Session.GetString("Role");
            if (role != "Patient")
            {
                return RedirectToAction("Login", "Account");
            }

    // Get the patient's data from database
    var userId = int.Parse(HttpContext.Session.GetString("UserId")!);
    var patient = await _context.Users.FindAsync(userId);
    
    // Pass the model to the view
    return View(patient);
}

        public IActionResult Privacy()
        {
            return View();
        }
    }
}