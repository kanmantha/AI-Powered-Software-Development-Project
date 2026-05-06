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
    /// Controller for managing DevOps Tasks
    /// Integrates with AI services for real-time DevOps suggestions
    /// </summary>
    public class DevOpsTasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AIServiceFactory _aiServiceFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DevOpsTasksController> _logger;
        private readonly ErrorLoggingService _errorLoggingService;

        /// <summary>
        /// Constructor - initializes dependencies via dependency injection
        /// </summary>
        public DevOpsTasksController(
            ApplicationDbContext context, 
            AIServiceFactory aiServiceFactory, 
            IConfiguration configuration,
            ILogger<DevOpsTasksController> logger,
            ErrorLoggingService errorLoggingService)
        {
            _context = context;
            _aiServiceFactory = aiServiceFactory;
            _configuration = configuration;
            _logger = logger;
            _errorLoggingService = errorLoggingService;
        }

        /// <summary>
        /// GET: DevOpsTasks
        /// Lists all DevOps tasks, optionally filtered by project
        /// </summary>
        public async Task<IActionResult> Index(int? projectId)
        {
            try
            {
                var query = _context.DevOpsTasks.Include(d => d.Project).AsQueryable();

                if (projectId.HasValue)
                {
                    query = query.Where(d => d.ProjectId == projectId);
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
                    nameof(DevOpsTasksController), 
                    nameof(Index), 
                    ex, 
                    $"projectId: {projectId}");
                return View("Error", new ErrorViewModel { Message = "Unable to load DevOps tasks." });
            }
        }

        /// <summary>
        /// GET: DevOpsTasks/Details/5
        /// Shows DevOps task details including AI suggestions
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var devOpsTask = await _context.DevOpsTasks
                    .Include(d => d.Project)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (devOpsTask == null) return NotFound();

                return View(devOpsTask);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(DevOpsTasksController), 
                    nameof(Details), 
                    ex, 
                    $"DevOpsTask ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to load DevOps task details." });
            }
        }

        /// <summary>
        /// GET: DevOpsTasks/Create
        /// Displays form to create a new DevOps task
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
                    nameof(DevOpsTasksController), 
                    nameof(Create), 
                    ex).Wait();
                return View("Error", new ErrorViewModel { Message = "Unable to load create form." });
            }
        }

        /// <summary>
        /// POST: DevOpsTasks/Create
        /// Creates a new DevOps task with AI-generated suggestions
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,TaskName,Description,TaskType,Status")] DevOpsTask devOpsTask)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Set creation timestamp
                    devOpsTask.CreatedAt = DateTime.Now;

                    // Call real-time AI for DevOps suggestions if project exists
                    var project = await _context.Projects.FindAsync(devOpsTask.ProjectId);
                    if (project != null)
                    {
                        try
                        {
                            // Create AI service based on project technology
                            var aiService = _aiServiceFactory.CreateService(
                                project.Technology ?? "", 
                                _configuration, 
                                HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>());
                            
                            // Call AI for DevOps suggestions
                            devOpsTask.AiSuggestion = await aiService.GenerateDevOpsSuggestionAsync(
                                devOpsTask.Description ?? "", 
                                devOpsTask.TaskType ?? "");
                        }
                        catch (Exception aiEx)
                        {
                            // Log AI service error but continue - don't fail the whole request
                            await _errorLoggingService.LogErrorAsync(
                                nameof(DevOpsTasksController), 
                                nameof(Create), 
                                aiEx, 
                                "AI suggestion failed, continuing without AI result");
                            devOpsTask.AiSuggestion = "AI suggestion failed. Please try again later.";
                        }
                    }
                    else
                    {
                        devOpsTask.AiSuggestion = "Project not found. Cannot generate AI suggestion.";
                    }

                    _context.Add(devOpsTask);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("DevOps task created: {TaskName}", devOpsTask.TaskName);
                    return RedirectToAction(nameof(Index), new { projectId = devOpsTask.ProjectId });
                }

                ViewBag.Projects = _context.Projects.ToList();
                return View(devOpsTask);
            }
            catch (DbUpdateException ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(DevOpsTasksController), 
                    nameof(Create), 
                    ex, 
                    $"Failed to save DevOps task: {devOpsTask.TaskName}");
                ModelState.AddModelError("", "Database error occurred while saving.");
                ViewBag.Projects = _context.Projects.ToList();
                return View(devOpsTask);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(DevOpsTasksController), 
                    nameof(Create), 
                    ex);
                ModelState.AddModelError("", "An unexpected error occurred.");
                ViewBag.Projects = _context.Projects.ToList();
                return View(devOpsTask);
            }
        }

        /// <summary>
        /// GET: DevOpsTasks/Edit/5
        /// Displays form to edit an existing DevOps task
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var devOpsTask = await _context.DevOpsTasks.FindAsync(id);
                if (devOpsTask == null) return NotFound();

                ViewBag.Projects = _context.Projects.ToList();
                return View(devOpsTask);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(DevOpsTasksController), 
                    nameof(Edit), 
                    ex, 
                    $"DevOpsTask ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to load DevOps task for editing." });
            }
        }

        /// <summary>
        /// POST: DevOpsTasks/Edit/5
        /// Updates an existing DevOps task
        /// Handles concurrency conflicts
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,TaskName,Description,AiSuggestion,TaskType,Status,CreatedAt")] DevOpsTask devOpsTask)
        {
            if (id != devOpsTask.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(devOpsTask);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { projectId = devOpsTask.ProjectId });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!DevOpsTaskExists(devOpsTask.Id)) return NotFound();
                    await _errorLoggingService.LogErrorAsync(
                        nameof(DevOpsTasksController), 
                        nameof(Edit), 
                        ex);
                    throw;
                }
                catch (Exception ex)
                {
                    await _errorLoggingService.LogErrorAsync(
                        nameof(DevOpsTasksController), 
                        nameof(Edit), 
                        ex);
                    ModelState.AddModelError("", "Unable to update DevOps task.");
                }
            }
            ViewBag.Projects = _context.Projects.ToList();
            return View(devOpsTask);
        }

        /// <summary>
        /// GET: DevOpsTasks/Delete/5
        /// Displays confirmation page for deleting a DevOps task
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var devOpsTask = await _context.DevOpsTasks
                    .Include(d => d.Project)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (devOpsTask == null) return NotFound();

                return View(devOpsTask);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(DevOpsTasksController), 
                    nameof(Delete), 
                    ex, 
                    $"DevOpsTask ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to load DevOps task for deletion." });
            }
        }

        /// <summary>
        /// POST: DevOpsTasks/Delete/5
        /// Deletes the specified DevOps task
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var devOpsTask = await _context.DevOpsTasks.FindAsync(id);
                if (devOpsTask != null)
                {
                    _context.DevOpsTasks.Remove(devOpsTask);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("DevOps task deleted: {TaskName}", devOpsTask.TaskName);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(DevOpsTasksController), 
                    nameof(DeleteConfirmed), 
                    ex, 
                    $"DevOpsTask ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to delete DevOps task." });
            }
        }

        /// <summary>
        /// Checks if a DevOps task exists by ID
        /// </summary>
        private bool DevOpsTaskExists(int id)
        {
            return _context.DevOpsTasks.Any(e => e.Id == id);
        }
    }
}
