using System;

namespace AIPoweredSoftwareDevelopment.Models
{
    public class DevOpsTask
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string? TaskName { get; set; }
        public string? Description { get; set; }
        public string? AiSuggestion { get; set; }
        public string? TaskType { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        public Project? Project { get; set; }
    }
}
