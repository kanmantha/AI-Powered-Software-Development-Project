using System;

namespace AIPoweredSoftwareDevelopment.Models
{
    public class TestCase
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string? TestName { get; set; }
        public string? TestDescription { get; set; }
        public string? AiGeneratedCode { get; set; }
        public string? TestType { get; set; }
        public string? Status { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime? LastRunAt { get; set; }
        
        public Project? Project { get; set; }
    }
}
