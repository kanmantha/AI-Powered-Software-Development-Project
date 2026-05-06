using System;

namespace AIPoweredSoftwareDevelopment.Models
{
    /// <summary>
    /// Model for logging application errors to SQLite database
    /// </summary>
    public class ErrorLog
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Controller where the error occurred
        /// </summary>
        public string? ControllerName { get; set; }
        
        /// <summary>
        /// Action method where the error occurred
        /// </summary>
        public string? ActionName { get; set; }
        
        /// <summary>
        /// Error message
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Full stack trace of the exception
        /// </summary>
        public string? StackTrace { get; set; }
        
        /// <summary>
        /// Type of exception (e.g., NullReferenceException, DbUpdateException)
        /// </summary>
        public string? ExceptionType { get; set; }
        
        /// <summary>
        /// Timestamp when the error occurred
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Additional context or user-friendly message
        /// </summary>
        public string? AdditionalInfo { get; set; }
    }
}
