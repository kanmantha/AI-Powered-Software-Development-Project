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
    /// Controller for managing Test Cases
    /// Integrates with AI services for real-time test generation
    /// </summary>
    public class TestCasesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AIServiceFactory _aiServiceFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TestCasesController> _logger;
        private readonly ErrorLoggingService _errorLoggingService;

        /// <summary>
        /// Constructor - initializes dependencies via dependency injection
        /// </summary>
        public TestCasesController(
            ApplicationDbContext context, 
            AIServiceFactory aiServiceFactory, 
            IConfiguration configuration,
            ILogger<TestCasesController> logger,
            ErrorLoggingService errorLoggingService)
        {
            _context = context;
            _aiServiceFactory = aiServiceFactory;
            _configuration = configuration;
            _logger = logger;
            _errorLoggingService = errorLoggingService;
        }

        /// <summary>
        /// GET: TestCases
        /// Lists all test cases, optionally filtered by project
        /// </summary>
        public async Task<IActionResult> Index(int? projectId)
        {
            try
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
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(TestCasesController), 
                    nameof(Index), 
                    ex, 
                    $"projectId: {projectId}");
                return View("Error", new ErrorViewModel { Message = "Unable to load test cases." });
            }
        }

        /// <summary>
        /// GET: TestCases/Details/5
        /// Shows test case details including AI-generated code
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var testCase = await _context.TestCases
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (testCase == null) return NotFound();

                return View(testCase);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(TestCasesController), 
                    nameof(Details), 
                    ex, 
                    $"TestCase ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to load test case details." });
            }
        }

        /// <summary>
        /// GET: TestCases/Create
        /// Displays form to create a new test case
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
                    nameof(TestCasesController), 
                    nameof(Create), 
                    ex).Wait();
                return View("Error", new ErrorViewModel { Message = "Unable to load create form." });
            }
        }

        /// <summary>
        /// POST: TestCases/Create
        /// Creates a new test case with AI-generated test code
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,TestName,TestDescription,TestType,Status")] TestCase testCase)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Set generation timestamp
                    testCase.GeneratedAt = DateTime.Now;

                    // Call real-time AI test generation if project exists
                    var project = await _context.Projects.FindAsync(testCase.ProjectId);
                    if (project != null)
                    {
                        try
                        {
                            // Create AI service based on project technology
                            var aiService = _aiServiceFactory.CreateService(
                                project.Technology ?? "", 
                                _configuration, 
                                HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>());
                            
                            // Call AI for test case generation
                            testCase.AiGeneratedCode = await aiService.GenerateTestCasesAsync(
                                testCase.TestDescription ?? "", 
                                testCase.TestType ?? "");
                        }
                        catch (Exception aiEx)
                        {
                            // Log AI service error but continue - don't fail the whole request
                            await _errorLoggingService.LogErrorAsync(
                                nameof(TestCasesController), 
                                nameof(Create), 
                                aiEx, 
                                "AI test generation failed, continuing without AI result");
                            testCase.AiGeneratedCode = "// AI test generation failed. Please try again later.";
                        }
                    }
                    else
                    {
                        testCase.AiGeneratedCode = "// AI test generation failed. Project not found.";
                    }

                    _context.Add(testCase);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Test case created: {TestName}", testCase.TestName);
                    return RedirectToAction(nameof(Index), new { projectId = testCase.ProjectId });
                }

                ViewBag.Projects = _context.Projects.ToList();
                return View(testCase);
            }
            catch (DbUpdateException ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(TestCasesController), 
                    nameof(Create), 
                    ex, 
                    $"Failed to save test case: {testCase.TestName}");
                ModelState.AddModelError("", "Database error occurred while saving.");
                ViewBag.Projects = _context.Projects.ToList();
                return View(testCase);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(TestCasesController), 
                    nameof(Create), 
                    ex);
                ModelState.AddModelError("", "An unexpected error occurred.");
                ViewBag.Projects = _context.Projects.ToList();
                return View(testCase);
            }
        }

        /// <summary>
        /// GET: TestCases/Edit/5
        /// Displays form to edit an existing test case
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var testCase = await _context.TestCases.FindAsync(id);
                if (testCase == null) return NotFound();

                ViewBag.Projects = _context.Projects.ToList();
                return View(testCase);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(TestCasesController), 
                    nameof(Edit), 
                    ex, 
                    $"TestCase ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to load test case for editing." });
            }
        }

        /// <summary>
        /// POST: TestCases/Edit/5
        /// Updates an existing test case
        /// Handles concurrency conflicts
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,TestName,TestDescription,AiGeneratedCode,TestType,Status,GeneratedAt")] TestCase testCase)
        {
            if (id != testCase.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(testCase);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { projectId = testCase.ProjectId });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!TestCaseExists(testCase.Id)) return NotFound();
                    await _errorLoggingService.LogErrorAsync(
                        nameof(TestCasesController), 
                        nameof(Edit), 
                        ex);
                    throw;
                }
                catch (Exception ex)
                {
                    await _errorLoggingService.LogErrorAsync(
                        nameof(TestCasesController), 
                        nameof(Edit), 
                        ex);
                    ModelState.AddModelError("", "Unable to update test case.");
                }
            }
            ViewBag.Projects = _context.Projects.ToList();
            return View(testCase);
        }

        /// <summary>
        /// GET: TestCases/Delete/5
        /// Displays confirmation page for deleting a test case
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var testCase = await _context.TestCases
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (testCase == null) return NotFound();

                return View(testCase);
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(TestCasesController), 
                    nameof(Delete), 
                    ex, 
                    $"TestCase ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to load test case for deletion." });
            }
        }

        /// <summary>
        /// POST: TestCases/Delete/5
        /// Deletes the specified test case
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var testCase = await _context.TestCases.FindAsync(id);
                if (testCase != null)
                {
                    _context.TestCases.Remove(testCase);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Test case deleted: {TestName}", testCase.TestName);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _errorLoggingService.LogErrorAsync(
                    nameof(TestCasesController), 
                    nameof(DeleteConfirmed), 
                    ex, 
                    $"TestCase ID: {id}");
                return View("Error", new ErrorViewModel { Message = "Unable to delete test case." });
            }
        }

        /// <summary>
        /// Checks if a test case exists by ID
        /// </summary>
        private bool TestCaseExists(int id)
        {
            return _context.TestCases.Any(e => e.Id == id);
        }
    }
}
