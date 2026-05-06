using System;

namespace AIPoweredSoftwareDevelopment.Models
{
    public class BugReport
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? AiDetectionResult { get; set; }
        public string? Severity { get; set; }
        public string? Status { get; set; }
        public DateTime DetectedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        
        public Project? Project { get; set; }
    }
}
