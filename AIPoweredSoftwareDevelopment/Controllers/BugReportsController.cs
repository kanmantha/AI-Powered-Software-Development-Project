using Microsoft.AspNetCore.Mvc;
using AIPoweredSoftwareDevelopment.Data;
using AIPoweredSoftwareDevelopment.Models;
using AIPoweredSoftwareDevelopment.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIPoweredSoftwareDevelopment.Controllers
{
    /// <summary>
    /// Controller for managing Bug Reports
    /// Integrates with AI services for real-time bug detection
    /// </summary>
    public class BugReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AIServiceFactory _aiServiceFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BugReportsController> _logger;
        private readonly ErrorLoggingService _errorLoggingService;

        /// <summary>
        /// Constructor - initializes dependencies via dependency injection
        /// </summary>
        public BugReportsController(
            ApplicationDbContext context, 
            AIServiceFactory aiServiceFactory, 
            IConfiguration configuration,
            ILogger<BugReportsController> logger,
            ErrorLoggingService errorLoggingService)
        {
            _context = context;
            _aiServiceFactory = aiServiceFactory;
            _configuration = configuration;
            _logger = logger;
            _errorLoggingService = errorLoggingService;
        }

        /// <summary>
        /// GET: BugReports
        /// Lists all bug reports, optionally filtered by project
        /// </summary>
        public async Task<IActionResult> Index(int? projectId)
        {
            try
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
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(BugReportsController), 
                    nameof(Index), 
                    ex, 
                    $"projectId: {projectId}");
                return View("Error", new ErrorViewModel { Message = "Unable to load bug reports." });
            }
        }

        /// <summary>
        /// GET: BugReports/Details/5
        /// Shows bug report details including AI detection result
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var bugReport = await _context.BugReports
                    .Include(b => b.Project)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (bugReport == null) return NotFound();

                return View(bugReport);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(BugReportsController), 
                    nameof(Details), 
                    ex, 
                    $"BugReport ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to load bug report details." });
            }
        }

        /// <summary>
        /// GET: BugReports/Create
        /// Displays form to create a new bug report
        /// </summary>
        public IActionResult Create(int? projectId)
        {
            try
            {
                ViewBag.ProjectId = projectId;
                ViewBag.Projects = _context.Projects.ToList();
                return View();
            }
            catch (Exception ex)
            {
                _errorLoggingService.LogErrorAsync(
                    nameof(BugReportsController), 
                    nameof(Create), 
                    ex).Wait();
                return View("Error", new ErrorViewModel { Message = "Unable to load create form." });
            }
        }

        /// <summary>
        /// POST: BugReports/Create
        /// Creates a new bug report with AI bug detection
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,Title,Description,Severity,Status")] BugReport bugReport)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Set detection timestamp
                    bugReport.DetectedAt = DateTime.Now;

                    // Call real-time AI bug detection if project exists
                    var project = await _context.Projects.FindAsync(bugReport.ProjectId);
                    if (project != null)
                    {
                        try
                        {
                            // Create AI service based on project technology
                            var aiService = _aiServiceFactory.CreateService(
                                project.Technology ?? "", 
                                _configuration, 
                                HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>());
                            
                            // Call AI for bug detection
                            bugReport.AiDetectionResult = await aiService.DetectBugsAsync(bugReport.Description ?? "");
                        }
                        catch (Exception aiEx)
                        {
                            // Log AI service error but continue - don't fail the whole request
                            await _errorLoggingService.LogErrorAsync(
                                nameof(BugReportsController), 
                                nameof(Create), 
                                aiEx, 
                                "AI bug detection failed, continuing without AI result");
                            bugReport.AiDetectionResult = "AI detection failed. Please try again later.";
                        }
                    }
                    else
                    {
                        bugReport.AiDetectionResult = "Project not found. Cannot perform AI detection.";
                    }

                    _context.Add(bugReport);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Bug report created: {Title}", bugReport.Title);
                    return RedirectToAction(nameof(Index), new { projectId = bugReport.ProjectId });
                }

                ViewBag.Projects = _context.Projects.ToList();
                return View(bugReport);
            }
            catch (DbUpdateException ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(BugReportsController), 
                    nameof(Create), 
                    ex, 
                    $"Failed to save bug report: {bugReport.Title}");
                ModelState.AddModelError("", "Database error occurred while saving bug report.");
                ViewBag.Projects = _context.Projects.ToList();
                return View(bugReport);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(BugReportsController), 
                    nameof(Create), 
                    ex);
                ModelState.AddModelError("", "An unexpected error occurred.");
                ViewBag.Projects = _context.Projects.ToList();
                return View(bugReport);
            }
        }

        /// <summary>
        /// GET: BugReports/Edit/5
        /// Displays form to edit an existing bug report
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var bugReport = await _context.BugReports.FindAsync(id);
                if (bugReport == null) return NotFound();

                ViewBag.Projects = _context.Projects.ToList();
                return View(bugReport);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(BugReportsController), 
                    nameof(Edit), 
                    ex, 
                    $"BugReport ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to load bug report for editing." });
            }
        }

        /// <summary>
        /// POST: BugReports/Edit/5
        /// Updates an existing bug report
        /// Handles concurrency conflicts
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,Title,Description,AiDetectionResult,Severity,Status,DetectedAt")] BugReport bugReport)
        {
            if (id != bugReport.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bugReport);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { projectId = bugReport.ProjectId });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!BugReportExists(bugReport.Id)) return NotFound();
                    await _errorLoggingService.LogErrorAsync(
                        nameof(BugReportsController), 
                        nameof(Edit), 
                        ex);
                    throw;
                }
                catch (Exception ex)
                {
                    await _errorLoggingService.LogErrorAsync(
                        nameof(BugReportsController), 
                        nameof(Edit), 
                        ex);
                    ModelState.AddModelError("", "Unable to update bug report.");
                }
            }
            ViewBag.Projects = _context.Projects.ToList();
            return View(bugReport);
        }

        /// <summary>
        /// GET: BugReports/Delete/5
        /// Displays confirmation page for deleting a bug report
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var bugReport = await _context.BugReports
                    .Include(b => b.Project)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (bugReport == null) return NotFound();

                return View(bugReport);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(BugReportsController), 
                    nameof(Delete), 
                    ex, 
                    $"BugReport ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to load bug report for deletion." });
            }
        }

        /// <summary>
        /// POST: BugReports/Delete/5
        /// Deletes the specified bug report
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var bugReport = await _context.BugReports.FindAsync(id);
                if (bugReport != null)
                {
                    _context.BugReports.Remove(bugReport);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Bug report deleted: {Title}", bugReport.Title);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(BugReportsController), 
                    nameof(DeleteConfirmed), 
                    ex, 
                    $"BugReport ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to delete bug report." });
            }
        }

        /// <summary>
        /// Checks if a bug report exists by ID
        /// </summary>
        private bool BugReportExists(int id)
        {
            return _context.BugReports.Any(e => e.Id == id);
        }
    }
}
