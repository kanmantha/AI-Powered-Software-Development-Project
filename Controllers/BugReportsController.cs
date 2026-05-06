using Microsoft.AspNetCore.Mvc;
using AIPoweredSoftwareDevelopment.Data;
using AIPoweredSoftwareDevelopment.Models;
using AIPoweredSoftwareDevelopment.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AIPoweredSoftwareDevelopment.Controllers
{
    public class BugReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AIServiceFactory _aiServiceFactory;
        private readonly IConfiguration _configuration;

        public BugReportsController(ApplicationDbContext context, AIServiceFactory aiServiceFactory, IConfiguration configuration)
        {
            _context = context;
            _aiServiceFactory = aiServiceFactory;
            _configuration = configuration;
        }

        // GET: BugReports
        public async Task<IActionResult> Index(int? projectId)
        {
            var query = _context.BugReports.Include(b => b.Project).AsQueryable();
            
            if (projectId.HasValue)
            {
                query = query.Where(b => b.ProjectId == projectId);
                ViewBag.ProjectId = projectId;
                ViewBag.ProjectName = await _context.Projects
                    .Where(p => p.Id == projectId)
                    .Select(p => p.Name)
                    .FirstOrDefaultAsync();
            }
            
            return View(await query.ToListAsync());
        }

        // GET: BugReports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bugReport = await _context.BugReports
                .Include(b => b.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (bugReport == null)
            {
                return NotFound();
            }

            return View(bugReport);
        }

        // GET: BugReports/Create
        public IActionResult Create(int? projectId)
        {
            ViewBag.ProjectId = projectId;
            ViewBag.Projects = _context.Projects.ToList();
            return View();
        }

        // POST: BugReports/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,Title,Description,Severity,Status")] BugReport bugReport)
        {
            if (ModelState.IsValid)
            {
                bugReport.DetectedAt = DateTime.Now;
                
                // Call real-time AI bug detection
                var project = await _context.Projects.FindAsync(bugReport.ProjectId);
                if (project != null)
                {
                    var aiService = _aiServiceFactory.CreateService(project.Technology, _configuration, HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>());
                    bugReport.AiDetectionResult = await aiService.DetectBugsAsync(bugReport.Description);
                }
                else
                {
                    bugReport.AiDetectionResult = "Project not found. Cannot perform AI detection.";
                }
                
                _context.Add(bugReport);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { projectId = bugReport.ProjectId });
            }
            
            ViewBag.Projects = _context.Projects.ToList();
            return View(bugReport);
        }

        // GET: BugReports/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bugReport = await _context.BugReports.FindAsync(id);
            if (bugReport == null)
            {
                return NotFound();
            }
            
            ViewBag.Projects = _context.Projects.ToList();
            return View(bugReport);
        }

        // POST: BugReports/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,Title,Description,AiDetectionResult,Severity,Status,DetectedAt")] BugReport bugReport)
        {
            if (id != bugReport.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bugReport);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BugReportExists(bugReport.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { projectId = bugReport.ProjectId });
            }
            
            ViewBag.Projects = _context.Projects.ToList();
            return View(bugReport);
        }

        // GET: BugReports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bugReport = await _context.BugReports
                .Include(b => b.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (bugReport == null)
            {
                return NotFound();
            }

            return View(bugReport);
        }

        // POST: BugReports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bugReport = await _context.BugReports.FindAsync(id);
            var projectId = bugReport?.ProjectId;
            
            if (bugReport != null)
            {
                _context.BugReports.Remove(bugReport);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { projectId = projectId });
        }

        private bool BugReportExists(int id)
        {
            return _context.BugReports.Any(e => e.Id == id);
        }
    }
}
