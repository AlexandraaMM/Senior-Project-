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

        [HttpGet]
        public IActionResult Login()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            // If user is already logged in, redirect to appropriate dashboard
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(role))
            {
                // User is already logged in - redirect to their dashboard
                if (role == "Admin")
                {
                    return RedirectToAction("AdminDashboard", "Home");
                }
                else if (role == "Doctor")
                {
                    return RedirectToAction("DoctorDashboard", "Home");
                }
                else if (role == "Patient")
                {
                    return RedirectToAction("PatientDashboard", "Home");
                }
            }
            
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

                return RedirectToAction("PatientDashboard", "Home");
            }

            return View(user);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ChangeMyPassword()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeMyPassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, user.PasswordHash))
            {
                ViewBag.Error = "Current password is incorrect";
                return View();
            }

            // Validate new password
            if (string.IsNullOrEmpty(NewPassword) || NewPassword.Length < 6)
            {
                ViewBag.Error = "New password must be at least 6 characters long";
                return View();
            }

            if (NewPassword != ConfirmPassword)
            {
                ViewBag.Error = "New passwords do not match";
                return View();
            }

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Password changed successfully!";
            
            // Redirect based on role
            if (role == "Admin")
            {
                return RedirectToAction("AdminDashboard", "Home");
            }
            else if (role == "Doctor")
            {
                return RedirectToAction("DoctorDashboard", "Home");
            }
            else
            {
                return RedirectToAction("PatientDashboard", "Home");
            }
        }

     
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateName(string FullName)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrWhiteSpace(FullName))
            {
                TempData["Error"] = "Name cannot be empty";
                return RedirectToAction("Settings");
            }

            user.FullName = FullName;
            
            
            HttpContext.Session.SetString("FullName", FullName);
            
            await _context.SaveChangesAsync();

            TempData["Success"] = "Name updated successfully!";
            return RedirectToAction("Settings");
        }
    }
}