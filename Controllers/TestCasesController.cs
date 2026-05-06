using Microsoft.AspNetCore.Mvc;
using AIPoweredSoftwareDevelopment.Data;
using AIPoweredSoftwareDevelopment.Models;
using AIPoweredSoftwareDevelopment.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AIPoweredSoftwareDevelopment.Controllers
{
    public class TestCasesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AIServiceFactory _aiServiceFactory;
        private readonly IConfiguration _configuration;

        public TestCasesController(ApplicationDbContext context, AIServiceFactory aiServiceFactory, IConfiguration configuration)
        {
            _context = context;
            _aiServiceFactory = aiServiceFactory;
            _configuration = configuration;
        }

        // GET: TestCases
        public async Task<IActionResult> Index(int? projectId)
        {
            var query = _context.TestCases.Include(t => t.Project).AsQueryable();
            
            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId);
                ViewBag.ProjectId = projectId;
                ViewBag.ProjectName = await _context.Projects
                    .Where(p => p.Id == projectId)
                    .Select(p => p.Name)
                    .FirstOrDefaultAsync();
            }
            
            return View(await query.ToListAsync());
        }

        // GET: TestCases/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testCase = await _context.TestCases
                .Include(t => t.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (testCase == null)
            {
                return NotFound();
            }

            return View(testCase);
        }

        // GET: TestCases/Create
        public IActionResult Create(int? projectId)
        {
            ViewBag.ProjectId = projectId;
            ViewBag.Projects = _context.Projects.ToList();
            return View();
        }

        // POST: TestCases/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,TestName,TestDescription,TestType,Status")] TestCase testCase)
        {
            if (ModelState.IsValid)
            {
                testCase.GeneratedAt = DateTime.Now;
                
                // Call real-time AI test generation
                var project = await _context.Projects.FindAsync(testCase.ProjectId);
                if (project != null)
                {
                    var aiService = _aiServiceFactory.CreateService(project.Technology, _configuration, HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>());
                    testCase.AiGeneratedCode = await aiService.GenerateTestCasesAsync(testCase.TestDescription, testCase.TestType);
                }
                else
                {
                    testCase.AiGeneratedCode = "// AI test generation failed. Project not found.";
                }
                
                _context.Add(testCase);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { projectId = testCase.ProjectId });
            }
            
            ViewBag.Projects = _context.Projects.ToList();
            return View(testCase);
        }

        // GET: TestCases/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testCase = await _context.TestCases.FindAsync(id);
            if (testCase == null)
            {
                return NotFound();
            }
            
            ViewBag.Projects = _context.Projects.ToList();
            return View(testCase);
        }

        // POST: TestCases/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,TestName,TestDescription,AiGeneratedCode,TestType,Status,GeneratedAt")] TestCase testCase)
        {
            if (id != testCase.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(testCase);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestCaseExists(testCase.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { projectId = testCase.ProjectId });
            }
            
            ViewBag.Projects = _context.Projects.ToList();
            return View(testCase);
        }

        // GET: TestCases/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testCase = await _context.TestCases
                .Include(t => t.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (testCase == null)
            {
                return NotFound();
            }

            return View(testCase);
        }

        // POST: TestCases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var testCase = await _context.TestCases.FindAsync(id);
            var projectId = testCase?.ProjectId;
            
            if (testCase != null)
            {
                _context.TestCases.Remove(testCase);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { projectId = projectId });
        }

        private bool TestCaseExists(int id)
        {
            return _context.TestCases.Any(e => e.Id == id);
        }
    }
}
