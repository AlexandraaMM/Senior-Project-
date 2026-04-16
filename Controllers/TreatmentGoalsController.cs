using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PracticeFlow.Data;
using PracticeFlow.Models;

namespace INF_SP.Controllers
{
    public class TreatmentGoalsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TreatmentGoalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // View all goals for a specific health problem
        public async Task<IActionResult> Index(int problemId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userId) || role != "Doctor")
                return RedirectToAction("Login", "Account");

            var problem = await _context.HealthProblems
                .Include(h => h.Patient)
                .FirstOrDefaultAsync(h => h.ProblemID == problemId);

            if (problem == null)
                return NotFound();

            var goals = await _context.TreatmentGoals
                .Where(g => g.ProblemID == problemId)
                .OrderBy(g => g.TargetDate)
                .ToListAsync();

            ViewBag.ProblemId = problemId;
            ViewBag.ProblemDescription = problem.ProblemDescription;
            ViewBag.PatientName = problem.Patient?.FullName;
            ViewBag.PatientId = problem.PatientID;
            return View(goals);
        }

        // Create new treatment goal
        public async Task<IActionResult> Create(int problemId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userId) || role != "Doctor")
                return RedirectToAction("Login", "Account");

            var problem = await _context.HealthProblems
                .Include(h => h.Patient)
                .FirstOrDefaultAsync(h => h.ProblemID == problemId);

            if (problem == null)
                return NotFound();

            ViewBag.ProblemId = problemId;
            ViewBag.ProblemDescription = problem.ProblemDescription;
            ViewBag.PatientName = problem.Patient?.FullName;
            return View();
        }

        // Save new treatment goal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TreatmentGoal goal)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userId) || role != "Doctor")
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                goal.SetDate = DateTime.Now;
                _context.TreatmentGoals.Add(goal);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Treatment goal added successfully.";
                return RedirectToAction("Index", new { problemId = goal.ProblemID });
            }

            var problem = await _context.HealthProblems
                .Include(h => h.Patient)
                .FirstOrDefaultAsync(h => h.ProblemID == goal.ProblemID);

            ViewBag.ProblemId = goal.ProblemID;
            ViewBag.ProblemDescription = problem?.ProblemDescription;
            ViewBag.PatientName = problem?.Patient?.FullName;
            return View(goal);
        }

        // Update progress on a goal
        [HttpPost]
        public async Task<IActionResult> UpdateProgress(int goalId, int progress)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userId) || role != "Doctor")
                return RedirectToAction("Login", "Account");

            var goal = await _context.TreatmentGoals.FindAsync(goalId);
            if (goal == null)
                return NotFound();

            goal.ProgressPercent = Math.Clamp(progress, 0, 100);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Progress updated.";
            return RedirectToAction("Index", new { problemId = goal.ProblemID });
        }
    }
}