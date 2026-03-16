using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PracticeFlow.Data;
using PracticeFlow.Models;
using System.Security.Cryptography;
using System.Text;

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
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes("Password123"));
                user.PasswordHash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }

            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["Success"] = "User created successfully! Default password is: Password123";
                return RedirectToAction("ViewUsers");
            }

            return View(user);
        }

    }
}