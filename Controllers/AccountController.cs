using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PracticeFlow.Models;
using PracticeFlow.Data;

namespace INF_SP.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string passwordHash)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(passwordHash))
            {
                ViewBag.Error = "Please enter both username and password";
                return View();
            }

            // Find user by username
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            // Verify password with BCrypt
            if (user == null || !BCrypt.Net.BCrypt.Verify(passwordHash, user.PasswordHash))
            {
                ViewBag.Error = "Invalid username or password";
                return View();
            }

            // Store user info in session 
            HttpContext.Session.SetString("UserId", user.UserID.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("FullName", user.FullName);

            switch (user.Role)
            {
                case "Admin":
                    return RedirectToAction("AdminDashboard", "Home");
                case "Doctor":
                    return RedirectToAction("DoctorDashboard", "Home");
                case "Patient":
                    return RedirectToAction("PatientDashboard", "Home");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user, string Password)
        {
            user.Role = "Patient";

            // Hash password 
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password);

            if (ModelState.IsValid)
            {
                // Check if username exists
                if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("Username", "Username already exists");
                    return View(user);
                }

                // Check if email exists
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email already registered");
                    return View(user);
                }

                // Add to database
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Auto-login after registration
                HttpContext.Session.SetString("UserId", user.UserID.ToString());
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("FullName", user.FullName);

                // Always redirect to Patient Dashboard 
                return RedirectToAction("PatientDashboard", "Home");
            }

            return View(user);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}