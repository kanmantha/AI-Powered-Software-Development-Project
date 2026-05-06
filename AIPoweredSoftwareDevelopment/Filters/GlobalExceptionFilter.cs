using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AIPoweredSoftwareDevelopment.Services;
using AIPoweredSoftwareDevelopment.Data;
using Microsoft.EntityFrameworkCore;

namespace AIPoweredSoftwareDevelopment.Filters
{
    /// <summary>
    /// Global exception filter that catches all unhandled exceptions
    /// Logs errors to database and displays user-friendly error page
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ErrorLoggingService _errorLoggingService;
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ErrorLoggingService errorLoggingService, ILogger<GlobalExceptionFilter> logger)
        {
            _errorLoggingService = errorLoggingService;
            _logger = logger;
        }

        /// <summary>
        /// Called when an unhandled exception occurs
        /// </summary>
        public void OnException(ExceptionContext context)
        {
            // Log the error to database
            var controllerName = context.RouteData.Values["controller"]?.ToString();
            var actionName = context.RouteData.Values["action"]?.ToString();
            
            // Log error asynchronously (fire and forget)
            Task.Run(async () =>
            {
                await _errorLoggingService.LogErrorAsync(controllerName, actionName, context.Exception, 
                    "Unhandled exception caught by global filter");
            });

            // Set result to error view
            var viewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
                new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                context.ModelState)
            {
                ["ErrorMessage"] = "An error occurred while processing your request.",
                ["ErrorId"] = Guid.NewGuid().ToString()
            };
            
            context.Result = new ViewResult
            {
                ViewName = "Error",
                ViewData = viewData
            };

            context.ExceptionHandled = true;
        }
    }
}
