using System;

namespace AIPoweredSoftwareDevelopment.Models
{
    public class CodeReview
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string? Title { get; set; }
        public string? CodeSnippet { get; set; }
        public string? AiReviewResult { get; set; }
        public string? ReviewType { get; set; }
        public string? Severity { get; set; }
        public DateTime ReviewedAt { get; set; }
        public string? Status { get; set; }
        
        public Project? Project { get; set; }
    }
}
