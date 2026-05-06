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
    /// Controller for managing Code Reviews
    /// Integrates with AI services for real-time code analysis
    /// </summary>
    public class CodeReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AIServiceFactory _aiServiceFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CodeReviewsController> _logger;
        private readonly ErrorLoggingService _errorLoggingService;

        public CodeReviewsController(
            ApplicationDbContext context, 
            AIServiceFactory aiServiceFactory, 
            IConfiguration configuration,
            ILogger<CodeReviewsController> logger,
            ErrorLoggingService errorLoggingService)
        {
            _context = context;
            _aiServiceFactory = aiServiceFactory;
            _configuration = configuration;
            _logger = logger;
            _errorLoggingService = errorLoggingService;
        }

        /// <summary>
        /// GET: CodeReviews
        /// Lists all code reviews, optionally filtered by project
        /// </summary>
        public async Task<IActionResult> Index(int? projectId)
        {
            try
            {
                // Build query with project include for related data
                var query = _context.CodeReviews.Include(c => c.Project).AsQueryable();
                
                // Filter by project if projectId is provided
                if (projectId.HasValue)
                {
                    query = query.Where(c => c.ProjectId == projectId);
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
                    nameof(CodeReviewsController), 
                    nameof(Index), 
                    ex, 
                    $"projectId: {projectId}");
                return View("Error", new ErrorViewModel { Message = "Unable to load code reviews." });
            }
        }

        /// <summary>
        /// GET: CodeReviews/Details/5
        /// Shows code review details including AI analysis result
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var codeReview = await _context.CodeReviews
                    .Include(c => c.Project)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (codeReview == null)
                {
                    return NotFound();
                }

                return View(codeReview);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(CodeReviewsController), 
                    nameof(Details), 
                    ex, 
                    $"CodeReview ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to load code review details." });
            }
        }

        /// <summary>
        /// GET: CodeReviews/Create
        /// Displays form to create a new code review
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
                    nameof(CodeReviewsController), 
                    nameof(Create), 
                    ex).Wait();
                return View("Error", new ErrorViewModel { Message = "Unable to load create form." });
            }
        }

        /// <summary>
        /// POST: CodeReviews/Create
        /// Creates a new code review with AI analysis
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,Title,CodeSnippet,ReviewType,Severity,Status")] CodeReview codeReview)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Set review timestamp
                    codeReview.ReviewedAt = DateTime.Now;
                    
                    // Call real-time AI analysis if project exists
                    var project = await _context.Projects.FindAsync(codeReview.ProjectId);
                    if (project != null)
                    {
                        try
                        {
                            // Create AI service based on project technology
                            var aiService = _aiServiceFactory.CreateService(
                                project.Technology ?? "", 
                                _configuration, 
                                HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>());
                            
                            // Call AI for code analysis
                            codeReview.AiReviewResult = await aiService.AnalyzeCodeAsync(
                                codeReview.CodeSnippet ?? "", 
                                codeReview.ReviewType ?? "", 
                                project.Technology ?? "");
                        }
                        catch (Exception aiEx)
                        {
                            // Log AI service error but continue - don't fail the whole request
                            await _errorLoggingService.LogErrorAsync(
                                nameof(CodeReviewsController), 
                                nameof(Create), 
                                aiEx, 
                                "AI analysis failed, continuing without AI result");
                            codeReview.AiReviewResult = "AI analysis failed. Please try again later.";
                        }
                    }
                    else
                    {
                        codeReview.AiReviewResult = "Project not found. Cannot perform AI analysis.";
                    }
                    
                    _context.Add(codeReview);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Code review created: {Title}", codeReview.Title);
                    return RedirectToAction(nameof(Index), new { projectId = codeReview.ProjectId });
                }
                
                ViewBag.Projects = _context.Projects.ToList();
                return View(codeReview);
            }
            catch (DbUpdateException ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(CodeReviewsController), 
                    nameof(Create), 
                    ex, 
                    $"Failed to save code review: {codeReview.Title}");
                ModelState.AddModelError("", "Database error occurred while saving.");
                ViewBag.Projects = _context.Projects.ToList();
                return View(codeReview);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(CodeReviewsController), 
                    nameof(Create), 
                    ex);
                ModelState.AddModelError("", "An unexpected error occurred.");
                ViewBag.Projects = _context.Projects.ToList();
                return View(codeReview);
            }
        }

        // GET: CodeReviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null) return NotFound();
                
                var codeReview = await _context.CodeReviews.FindAsync(id);
                if (codeReview == null) return NotFound();
                
                ViewBag.Projects = _context.Projects.ToList();
                return View(codeReview);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(CodeReviewsController), 
                    nameof(Edit), 
                    ex, 
                    $"CodeReview ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to load code review for editing." });
            }
        }

        // POST: CodeReviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,Title,CodeSnippet,ReviewType,Severity,Status,AiReviewResult")] CodeReview codeReview)
        {
            if (id != codeReview.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(codeReview);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { projectId = codeReview.ProjectId });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!CodeReviewExists(codeReview.Id)) return NotFound();
                    await _errorLoggingService.LogErrorAsync(
                        nameof(CodeReviewsController), 
                        nameof(Edit), 
                        ex);
                    throw;
                }
                catch (Exception ex)
                {
                    await _errorLoggingService.LogErrorAsync(
                        nameof(CodeReviewsController), 
                        nameof(Edit), 
                        ex);
                    ModelState.AddModelError("", "Unable to update code review.");
                }
            }
            ViewBag.Projects = _context.Projects.ToList();
            return View(codeReview);
        }

        // GET: CodeReviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null) return NotFound();
                
                var codeReview = await _context.CodeReviews
                    .Include(c => c.Project)
                    .FirstOrDefaultAsync(m => m.Id == id);
                    
                if (codeReview == null) return NotFound();
                
                return View(codeReview);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(CodeReviewsController), 
                    nameof(Delete), 
                    ex, 
                    $"CodeReview ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to load code review for deletion." });
            }
        }

        // POST: CodeReviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var codeReview = await _context.CodeReviews.FindAsync(id);
                if (codeReview != null)
                {
                    _context.CodeReviews.Remove(codeReview);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Code review deleted: {Title}", codeReview.Title);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(CodeReviewsController), 
                    nameof(DeleteConfirmed), 
                    ex, 
                    $"CodeReview ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to delete code review." });
            }
        }

        private bool CodeReviewExists(int id)
        {
            return _context.CodeReviews.Any(e => e.Id == id);
        }
    }
}
