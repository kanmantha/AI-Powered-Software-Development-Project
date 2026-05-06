using Microsoft.AspNetCore.Mvc;
using AIPoweredSoftwareDevelopment.Data;
using Microsoft.EntityFrameworkCore;

namespace AIPoweredSoftwareDevelopment.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ProjectCount = await _context.Projects.CountAsync();
        ViewBag.CodeReviewCount = await _context.CodeReviews.CountAsync();
        ViewBag.BugReportCount = await _context.BugReports.CountAsync();
        ViewBag.TestCaseCount = await _context.TestCases.CountAsync();
        ViewBag.DevOpsTaskCount = await _context.DevOpsTasks.CountAsync();
        
        ViewBag.RecentProjects = await _context.Projects
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync();
            
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
