using System;
using System.Collections.Generic;

namespace AIPoweredSoftwareDevelopment.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? RepositoryUrl { get; set; }
        public string? Technology { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public ICollection<CodeReview>? CodeReviews { get; set; }
        public ICollection<BugReport>? BugReports { get; set; }
        public ICollection<TestCase>? TestCases { get; set; }
        public ICollection<DevOpsTask>? DevOpsTasks { get; set; }
    }
}
