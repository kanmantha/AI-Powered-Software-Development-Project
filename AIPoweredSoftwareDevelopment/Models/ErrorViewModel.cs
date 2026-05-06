namespace AIPoweredSoftwareDevelopment.Models
{
    /// <summary>
    /// ViewModel for displaying error messages to users
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// User-friendly error message to display
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Unique error ID for tracking
        /// </summary>
        public string? ErrorId { get; set; }

        /// <summary>
        /// Technical details (only shown in development)
        /// </summary>
        public string? TechnicalDetails { get; set; }

        /// <summary>
        /// Whether to show technical details
        /// </summary>
        public bool ShowTechnicalDetails { get; set; }
    }
}
