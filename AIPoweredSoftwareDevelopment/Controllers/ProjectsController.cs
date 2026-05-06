using Microsoft.AspNetCore.Mvc;
using AIPoweredSoftwareDevelopment.Data;
using AIPoweredSoftwareDevelopment.Models;
using Microsoft.EntityFrameworkCore;
using AIPoweredSoftwareDevelopment.Services; // For ErrorLoggingService
using Microsoft.Extensions.Logging; // For ILogger

namespace AIPoweredSoftwareDevelopment.Controllers
{
    /// <summary>
    /// Controller for managing Projects
    /// Handles CRUD operations and displays project details
    /// </summary>
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ErrorLoggingService _errorLoggingService;
        private readonly ILogger<ProjectsController> _logger;

        /// <summary>
        /// Constructor - initializes dependencies via dependency injection
        /// </summary>
        public ProjectsController(ApplicationDbContext context, ErrorLoggingService errorLoggingService, ILogger<ProjectsController> logger)
        {
            _context = context;
            _errorLoggingService = errorLoggingService;
            _logger = logger;
        }

        /// <summary>
        /// GET: Projects
        /// Displays list of all projects
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading Projects Index page");
                return View(await _context.Projects.ToListAsync());
            }
            catch (Exception ex)
            {
                // Log error to database and continue to show error view
                await _errorLoggingService.LogErrorAsync(nameof(ProjectsController), nameof(Index), ex);
                // Return error view with user-friendly message
                return View("Error", new ErrorViewModel { Message = "Unable to load projects. Please try again later." });
            }
        }

        /// <summary>
        /// GET: Projects/Details/5
        /// Shows project details including related entities
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                // Eager load related entities to avoid N+1 queries
                var project = await _context.Projects
                    .Include(p => p.CodeReviews)
                    .Include(p => p.BugReports)
                    .Include(p => p.TestCases)
                    .Include(p => p.DevOpsTasks)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (project == null)
                {
                    return NotFound();
                }

                return View(project);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(nameof(ProjectsController), nameof(Details), ex, $"Project ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to load project details." });
            }
        }

        /// <summary>
        /// GET: Projects/Create
        /// Displays form to create a new project
        /// </summary>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// POST: Projects/Create
        /// Creates a new project in the database
        /// Includes validation and error handling
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Set creation timestamp
                    project.CreatedAt = DateTime.Now;
                    _context.Add(project);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Project created successfully: {ProjectName}", project.Name);
                    return RedirectToAction(nameof(Index));
                }

                // Log validation errors for debugging
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogWarning("Validation Error: {ErrorMessage}", error.ErrorMessage);
                    }
                }

                // Return view with validation errors
                return View(project);
            }
            catch (DbUpdateException ex)
            {
                // Database update failed - log detailed error
                await _errorLoggingService.LogErrorAsync(nameof(ProjectsController), nameof(Create), ex, 
                    $"Failed to save project: {project.Name}");
                ModelState.AddModelError("", "Unable to save project. Database error occurred.");
                return View(project);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(nameof(ProjectsController), nameof(Create), ex);
                ModelState.AddModelError("", "An unexpected error occurred.");
                return View(project);
            }
        }

        /// <summary>
        /// GET: Projects/Edit/5
        /// Displays form to edit an existing project
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var project = await _context.Projects.FindAsync(id);
                if (project == null)
                {
                    return NotFound();
                }
                return View(project);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(nameof(ProjectsController), nameof(Edit), ex, $"Project ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to load project for editing." });
            }
        }

        /// <summary>
        /// POST: Projects/Edit/5
        /// Updates an existing project
        /// Handles concurrency conflicts
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,RepositoryUrl,Technology,CreatedAt")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set update timestamp
                    project.UpdatedAt = DateTime.Now;
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    // Handle concurrency conflict
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        await _errorLoggingService.LogErrorAsync(nameof(ProjectsController), nameof(Edit), ex, 
                            $"Concurrency conflict for project: {project.Name}");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    await _errorLoggingService.LogErrorAsync(nameof(ProjectsController), nameof(Edit), ex);
                    ModelState.AddModelError("", "Unable to update project.");
                    return View(project);
                }
            }
            return View(project);
        }

        /// <summary>
        /// GET: Projects/Delete/5
        /// Displays confirmation page for deleting a project
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var project = await _context.Projects
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (project == null)
                {
                    return NotFound();
                }

                return View(project);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(nameof(ProjectsController), nameof(Delete), ex, $"Project ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to load project for deletion." });
            }
        }

        /// <summary>
        /// POST: Projects/Delete/5
        /// Deletes the specified project
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var project = await _context.Projects.FindAsync(id);
                if (project != null)
                {
                    _context.Projects.Remove(project);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Project deleted: {ProjectName}", project.Name);
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                await _errorLoggingService.LogErrorAsync(nameof(ProjectsController), nameof(DeleteConfirmed), ex, 
                    $"Failed to delete project ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to delete project. It may have related records." });
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(nameof(ProjectsController), nameof(DeleteConfirmed), ex);
                return View("Error", new ErrorViewModel { Message = "An error occurred while deleting." });
            }
        }

        /// <summary>
        /// Checks if a project exists by ID
        /// </summary>
        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
