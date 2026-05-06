using Microsoft.AspNetCore.Mvc;
using AIPoweredSoftwareDevelopment.Data;
using AIPoweredSoftwareDevelopment.Models;
using AIPoweredSoftwareDevelopment.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AIPoweredSoftwareDevelopment.Controllers
{
    public class CodeReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AIServiceFactory _aiServiceFactory;
        private readonly IConfiguration _configuration;

        public CodeReviewsController(ApplicationDbContext context, AIServiceFactory aiServiceFactory, IConfiguration configuration)
        {
            _context = context;
            _aiServiceFactory = aiServiceFactory;
            _configuration = configuration;
        }

        // GET: CodeReviews
        public async Task<IActionResult> Index(int? projectId)
        {
            var query = _context.CodeReviews.Include(c => c.Project).AsQueryable();
            
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

        // GET: CodeReviews/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: CodeReviews/Create
        public IActionResult Create(int? projectId)
        {
            ViewBag.ProjectId = projectId;
            ViewBag.Projects = _context.Projects.ToList();
            return View();
        }

        // POST: CodeReviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,Title,CodeSnippet,ReviewType,Severity,Status")] CodeReview codeReview)
        {
            if (ModelState.IsValid)
            {
                codeReview.ReviewedAt = DateTime.Now;
                
                // Call real-time AI analysis
                var project = await _context.Projects.FindAsync(codeReview.ProjectId);
                if (project != null)
                {
                    var aiService = _aiServiceFactory.CreateService(project.Technology, _configuration, HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>());
                    codeReview.AiReviewResult = await aiService.AnalyzeCodeAsync(codeReview.CodeSnippet, codeReview.ReviewType, project.Technology);
                }
                else
                {
                    codeReview.AiReviewResult = "Project not found. Cannot perform AI analysis.";
                }
                
                _context.Add(codeReview);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { projectId = codeReview.ProjectId });
            }
            
            ViewBag.Projects = _context.Projects.ToList();
            return View(codeReview);
        }

        // GET: CodeReviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var codeReview = await _context.CodeReviews.FindAsync(id);
            if (codeReview == null)
            {
                return NotFound();
            }
            
            ViewBag.Projects = _context.Projects.ToList();
            return View(codeReview);
        }

        // POST: CodeReviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,Title,CodeSnippet,AiReviewResult,ReviewType,Severity,Status,ReviewedAt")] CodeReview codeReview)
        {
            if (id != codeReview.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(codeReview);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CodeReviewExists(codeReview.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { projectId = codeReview.ProjectId });
            }
            
            ViewBag.Projects = _context.Projects.ToList();
            return View(codeReview);
        }

        // GET: CodeReviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: CodeReviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var codeReview = await _context.CodeReviews.FindAsync(id);
            var projectId = codeReview?.ProjectId;
            
            if (codeReview != null)
            {
                _context.CodeReviews.Remove(codeReview);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { projectId = projectId });
        }

        private bool CodeReviewExists(int id)
        {
            return _context.CodeReviews.Any(e => e.Id == id);
        }
    }
}
