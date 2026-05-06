using AIPoweredSoftwareDevelopment.Data;
using AIPoweredSoftwareDevelopment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPoweredSoftwareDevelopment.Services
{
    /// <summary>
    /// Service for logging application errors to SQLite database
    /// </summary>
    public class ErrorLoggingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ErrorLoggingService> _logger;

        public ErrorLoggingService(ApplicationDbContext context, ILogger<ErrorLoggingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Logs an error to the database
        /// </summary>
        /// <param name="controllerName">Name of the controller where error occurred</param>
        /// <param name="actionName">Name of the action where error occurred</param>
        /// <param name="exception">The exception that occurred</param>
        /// <param name="additionalInfo">Additional context information</param>
        public async Task LogErrorAsync(string? controllerName, string? actionName, Exception exception, string? additionalInfo = null)
        {
            try
            {
                var errorLog = new ErrorLog
                {
                    ControllerName = controllerName,
                    ActionName = actionName,
                    ErrorMessage = exception.Message,
                    StackTrace = exception.StackTrace,
                    ExceptionType = exception.GetType().Name,
                    Timestamp = DateTime.Now,
                    AdditionalInfo = additionalInfo
                };

                _context.ErrorLogs.Add(errorLog);
                await _context.SaveChangesAsync();
                
                // Also log to standard logger
                _logger.LogError(exception, "Error in {Controller}/{Action}: {Message}", 
                    controllerName, actionName, exception.Message);
            }
            catch (Exception ex)
            {
                // If logging fails, at least write to debug output
                _logger.LogCritical(ex, "Failed to log error to database");
                System.Diagnostics.Debug.WriteLine($"Failed to log error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets recent error logs for display
        /// </summary>
        public async Task<List<ErrorLog>> GetRecentErrorsAsync(int count = 50)
        {
            return await _context.ErrorLogs
                .OrderByDescending(e => e.Timestamp)
                .Take(count)
                .ToListAsync<ErrorLog>();
        }
    }
}
