using Microsoft.AspNetCore.Mvc;
using AIPoweredSoftwareDevelopment.Data;
using Microsoft.EntityFrameworkCore;
using AIPoweredSoftwareDevelopment.Services;
using Microsoft.Extensions.Logging;
using AIPoweredSoftwareDevelopment.Models;

namespace AIPoweredSoftwareDevelopment.Controllers
{
    /// <summary>
    /// Home Controller - handles main application pages
    /// Displays dashboard with project statistics
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly ErrorLoggingService _errorLoggingService;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, ErrorLoggingService errorLoggingService)
        {
            _context = context;
            _logger = logger;
            _errorLoggingService = errorLoggingService;
        }

        /// <summary>
        /// GET: Home/Index
        /// Displays dashboard with project counts and recent projects
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading Home Index page");
                
                // Get counts for dashboard statistics
                ViewBag.ProjectCount = await _context.Projects.CountAsync();
                ViewBag.CodeReviewCount = await _context.CodeReviews.CountAsync();
                ViewBag.BugReportCount = await _context.BugReports.CountAsync();
                ViewBag.TestCaseCount = await _context.TestCases.CountAsync();
                ViewBag.DevOpsTaskCount = await _context.DevOpsTasks.CountAsync();
                
                // Get 5 most recent projects for display
                ViewBag.RecentProjects = await _context.Projects
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync();
                    
                return View();
            }
            catch (Exception ex)
            {
                // Log error to database
                await _errorLoggingService.LogErrorAsync(
                    nameof(HomeController), 
                    nameof(Index), 
                    ex, 
                    "Failed to load dashboard statistics");
                
                // Return error view with user-friendly message
                return View("Error", new ErrorViewModel 
                { 
                    Message = "Unable to load dashboard. Please try again later." 
                });
            }
        }

        /// <summary>
        /// GET: Home/Privacy
        /// Displays privacy policy page
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
