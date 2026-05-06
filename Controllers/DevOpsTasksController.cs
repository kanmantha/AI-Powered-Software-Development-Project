using Microsoft.AspNetCore.Mvc;
using AIPoweredSoftwareDevelopment.Data;
using AIPoweredSoftwareDevelopment.Models;
using AIPoweredSoftwareDevelopment.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AIPoweredSoftwareDevelopment.Controllers
{
    public class DevOpsTasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AIServiceFactory _aiServiceFactory;
        private readonly IConfiguration _configuration;

        public DevOpsTasksController(ApplicationDbContext context, AIServiceFactory aiServiceFactory, IConfiguration configuration)
        {
            _context = context;
            _aiServiceFactory = aiServiceFactory;
            _configuration = configuration;
        }

        // GET: DevOpsTasks
        public async Task<IActionResult> Index(int? projectId)
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

        // GET: DevOpsTasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var devOpsTask = await _context.DevOpsTasks
                .Include(d => d.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (devOpsTask == null)
            {
                return NotFound();
            }

            return View(devOpsTask);
        }

        // GET: DevOpsTasks/Create
        public IActionResult Create(int? projectId)
        {
            ViewBag.ProjectId = projectId;
            ViewBag.Projects = _context.Projects.ToList();
            return View();
        }

        // POST: DevOpsTasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,TaskName,Description,TaskType,Status")] DevOpsTask devOpsTask)
        {
            if (ModelState.IsValid)
            {
                devOpsTask.CreatedAt = DateTime.Now;
                
                // Call real-time AI for DevOps suggestions
                var project = await _context.Projects.FindAsync(devOpsTask.ProjectId);
                if (project != null)
                {
                    var aiService = _aiServiceFactory.CreateService(project.Technology, _configuration, HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>());
                    devOpsTask.AiSuggestion = await aiService.GenerateDevOpsSuggestionAsync(devOpsTask.Description, devOpsTask.TaskType);
                }
                else
                {
                    devOpsTask.AiSuggestion = "Project not found. Cannot generate AI suggestion.";
                }
                
                _context.Add(devOpsTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { projectId = devOpsTask.ProjectId });
            }
            
            ViewBag.Projects = _context.Projects.ToList();
            return View(devOpsTask);
        }

        // GET: DevOpsTasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var devOpsTask = await _context.DevOpsTasks.FindAsync(id);
            if (devOpsTask == null)
            {
                return NotFound();
            }
            
            ViewBag.Projects = _context.Projects.ToList();
            return View(devOpsTask);
        }

        // POST: DevOpsTasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,TaskName,Description,AiSuggestion,TaskType,Status,CreatedAt")] DevOpsTask devOpsTask)
        {
            if (id != devOpsTask.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(devOpsTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DevOpsTaskExists(devOpsTask.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { projectId = devOpsTask.ProjectId });
            }
            
            ViewBag.Projects = _context.Projects.ToList();
            return View(devOpsTask);
        }

        // GET: DevOpsTasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var devOpsTask = await _context.DevOpsTasks
                .Include(d => d.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (devOpsTask == null)
            {
                return NotFound();
            }

            return View(devOpsTask);
        }

        // POST: DevOpsTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var devOpsTask = await _context.DevOpsTasks.FindAsync(id);
            var projectId = devOpsTask?.ProjectId;
            
            if (devOpsTask != null)
            {
                _context.DevOpsTasks.Remove(devOpsTask);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { projectId = projectId });
        }

        private bool DevOpsTaskExists(int id)
        {
            return _context.DevOpsTasks.Any(e => e.Id == id);
        }
    }
}
