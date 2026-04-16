using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PracticeFlow.Data;
using PracticeFlow.Models;
using BCrypt.Net;

namespace INF_SP.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

       
        public async Task<IActionResult> ViewUsers()
        {
            // Check if user is logged in and is admin
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId) || role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var users = await _context.Users
                .OrderBy(u => u.Role)
                .ThenBy(u => u.FullName)
                .ToListAsync();

            return View(users);
        }

        
        public IActionResult CreateUser()
        {
            // Check if user is logged in and is admin
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId) || role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(User user)
        {
            // Check if user is logged in and is admin
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId) || role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if username already exists
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "Username already exists");
                return View(user);
            }

            // Check if email already exists
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(user);
            }

            // Hash the password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123");

            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["Success"] = "User created successfully! Default password is: Password123";
                return RedirectToAction("ViewUsers");
            }

            return View(user);
        }

        // GET: Edit user
        public async Task<IActionResult> EditUser(int id)
        {
            // Check if user is logged in and is admin
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId) || role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            
            if (id.ToString() == userId)
            {
                TempData["Error"] = "You cannot edit your own account! Use Settings to update your information.";
                return RedirectToAction("ViewUsers");
            }

            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction("ViewUsers");
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, User user)
        {
            // Check if user is logged in and is admin
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId) || role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            if (id.ToString() == userId)
            {
                TempData["Error"] = "You cannot edit your own account! Use Settings to update your information.";
                return RedirectToAction("ViewUsers");
            }

            if (id != user.UserID)
            {
                return NotFound();
            }

            // Check if username already exists 
            if (_context.Users.Any(u => u.Username == user.Username && u.UserID != id))
            {
                ModelState.AddModelError("Username", "Username already exists");
                return View(user);
            }

            // Check if email already exists 
            if (_context.Users.Any(u => u.Email == user.Email && u.UserID != id))
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(user);
            }

            
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Update fields but keep the existing password
            existingUser.Username = user.Username;
            existingUser.FullName = user.FullName;
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;
            existingUser.DateOfBirth = user.DateOfBirth;

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "User updated successfully!";
                return RedirectToAction("ViewUsers");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(e => e.UserID == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

     
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Check if user is logged in and is admin
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(userId) || role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction("ViewUsers");
            }

            // Prevent admin from deleting themselves
            if (user.UserID.ToString() == userId)
            {
                TempData["Error"] = "You cannot delete your own account!";
                return RedirectToAction("ViewUsers");
            }

            // 1. Delete prescriptions for this user
            var prescriptions = await _context.Prescriptions
                .Where(p => p.PatientID == id)
                .ToListAsync();
            _context.Prescriptions.RemoveRange(prescriptions);

            // 2. Delete vitals logs
            var vitals = await _context.VitalsLogs
                .Where(v => v.PatientID == id)
                .ToListAsync();
            _context.VitalsLogs.RemoveRange(vitals);

            var medicalRecords = await _context.MedicalRecords
                .Where(m => m.PatientId == id || m.DoctorId == id)
                .ToListAsync();
            _context.MedicalRecords.RemoveRange(medicalRecords);

            var appointments = await _context.Appointments
                .Where(a => a.PatientId == id || a.DoctorId == id)
                .ToListAsync();
            _context.Appointments.RemoveRange(appointments);

            // 5. Finally delete the user
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"User '{user.Username}' deleted successfully!";
            return RedirectToAction("ViewUsers");
        }
        [HttpGet]
        public async Task<IActionResult> ChangePassword(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            
            if (id.ToString() == userId)
            {
                TempData["Error"] = "You cannot change your own password here! Use Settings to change your password.";
                return RedirectToAction("ViewUsers");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.UserId = user.UserID;
            ViewBag.Username = user.Username;
            ViewBag.FullName = user.FullName;
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int UserId, string NewPassword, string ConfirmPassword)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (UserId.ToString() == userId)
            {
                TempData["Error"] = "You cannot change your own password here! Use Settings to change your password.";
                return RedirectToAction("ViewUsers");
            }
            
            if (string.IsNullOrEmpty(NewPassword) || NewPassword.Length < 6)
            {
                ViewBag.Error = "Password must be at least 6 characters long";
                var user = await _context.Users.FindAsync(UserId);
                ViewBag.UserId = user!.UserID;
                ViewBag.Username = user.Username;
                ViewBag.FullName = user.FullName;
                return View();
            }

            if (NewPassword != ConfirmPassword)
            {
                ViewBag.Error = "Passwords do not match";
                var user = await _context.Users.FindAsync(UserId);
                ViewBag.UserId = user!.UserID;
                ViewBag.Username = user.Username;
                ViewBag.FullName = user.FullName;
                return View();
            }

            var userToUpdate = await _context.Users.FindAsync(UserId);
            if (userToUpdate == null)
            {
                return NotFound();
            }

            userToUpdate.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            
            await _context.SaveChangesAsync();
            
            TempData["Success"] = $"Password changed successfully for {userToUpdate.Username}";
            return RedirectToAction("ViewUsers");
        }

    }
}