using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PracticeFlow.Data;
using PracticeFlow.Models;

namespace INF_SP.Controllers
{
    public class HealthProblemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HealthProblemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Doctor views all health problems for a specific patient
        public async Task<IActionResult> Index(int patientId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userId) || role != "Doctor")
                return RedirectToAction("Login", "Account");

            var patient = await _context.Users.FindAsync(patientId);
            if (patient == null)
                return NotFound();

            var problems = await _context.HealthProblems
                .Where(h => h.PatientID == patientId)
                .OrderByDescending(h => h.DiagnosedDate)
                .ToListAsync();

            ViewBag.PatientName = patient.FullName;
            ViewBag.PatientId = patientId;
            return View(problems);
        }

        // Create new health problem
        public async Task<IActionResult> Create(int patientId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userId) || role != "Doctor")
                return RedirectToAction("Login", "Account");

            var patient = await _context.Users.FindAsync(patientId);
            if (patient == null)
                return NotFound();

            ViewBag.PatientId = patientId;
            ViewBag.PatientName = patient.FullName;
            return View();
        }

        // Save new health problem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HealthProblem problem)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userId) || role != "Doctor")
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                problem.DiagnosedDate = DateTime.Now;
                problem.Status = "Active";
                _context.HealthProblems.Add(problem);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Health problem recorded successfully.";
                return RedirectToAction("Index", new { patientId = problem.PatientID });
            }

            var patient = await _context.Users.FindAsync(problem.PatientID);
            ViewBag.PatientId = problem.PatientID;
            ViewBag.PatientName = patient?.FullName;
            return View(problem);
        }

        // Mark problem as resolved
        public async Task<IActionResult> Resolve(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userId) || role != "Doctor")
                return RedirectToAction("Login", "Account");

            var problem = await _context.HealthProblems.FindAsync(id);
            if (problem == null)
                return NotFound();

            problem.Status = "Resolved";
            problem.ResolvedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Problem marked as resolved.";
            return RedirectToAction("Index", new { patientId = problem.PatientID });
        }

        public async Task<IActionResult> MyHealthProblems()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userId) || role != "Patient")
                return RedirectToAction("Login", "Account");

            var patientId = int.Parse(userId);

            var problems = await _context.HealthProblems
                .Where(h => h.PatientID == patientId)
                .OrderByDescending(h => h.DiagnosedDate)
                .ToListAsync();

            return View(problems);
        }

        // Patient views goals for a specific problem 
        public async Task<IActionResult> MyGoals(int problemId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userId) || role != "Patient")
                return RedirectToAction("Login", "Account");

            var patientId = int.Parse(userId);

            var problem = await _context.HealthProblems
                .FirstOrDefaultAsync(h => h.ProblemID == problemId 
                                    && h.PatientID == patientId);

            if (problem == null)
                return NotFound();

            var goals = await _context.TreatmentGoals
                .Where(g => g.ProblemID == problemId)
                .OrderBy(g => g.TargetDate)
                .ToListAsync();

            ViewBag.ProblemDescription = problem.ProblemDescription;
            ViewBag.ProblemStatus = problem.Status;
            return View(goals);
        }   
    }
}