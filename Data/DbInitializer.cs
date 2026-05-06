using Microsoft.EntityFrameworkCore;
using AIPoweredSoftwareDevelopment.Models;

namespace AIPoweredSoftwareDevelopment.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if there are any projects already
            if (await context.Projects.AnyAsync())
            {
                return; // DB has been seeded
            }

            // Add sample projects
            var projects = new Project[]
            {
                new Project
                {
                    Name = "AI Code Review System",
                    Description = "Automated code review system using AI agents to detect bugs and suggest improvements",
                    RepositoryUrl = "https://github.com/example/ai-code-review",
                    Technology = "GitHub Copilot",
                    CreatedAt = DateTime.Now.AddDays(-30)
                },
                new Project
                {
                    Name = "AI Bug Detection Platform",
                    Description = "Platform for detecting bugs in code using machine learning algorithms",
                    RepositoryUrl = "https://github.com/example/ai-bug-detection",
                    Technology = "Claude Code",
                    CreatedAt = DateTime.Now.AddDays(-25)
                },
                new Project
                {
                    Name = "Test Case Generator",
                    Description = "AI-powered test case generation for automated testing workflows",
                    RepositoryUrl = "https://github.com/example/test-generator",
                    Technology = "Cursor AI",
                    CreatedAt = DateTime.Now.AddDays(-20)
                }
            };

            await context.Projects.AddRangeAsync(projects);
            await context.SaveChangesAsync();

            // Add sample code reviews
            var codeReviews = new CodeReview[]
            {
                new CodeReview
                {
                    ProjectId = 1,
                    Title = "Security Review - Authentication Module",
                    CodeSnippet = "public bool Authenticate(string username, string password) {\n    // Check credentials\n    return true;\n}",
                    AiReviewResult = "CRITICAL: Authentication bypass vulnerability detected. The method always returns true without validating credentials. Implement proper password hashing and validation.",
                    ReviewType = "Security",
                    Severity = "Critical",
                    Status = "Completed",
                    ReviewedAt = DateTime.Now.AddDays(-28)
                },
                new CodeReview
                {
                    ProjectId = 1,
                    Title = "Performance Review - Data Processing",
                    CodeSnippet = "foreach (var item in largeDataset) {\n    ProcessItem(item);\n}",
                    AiReviewResult = "WARNING: Nested loop detected in data processing. Consider using parallel processing or optimizing the algorithm for better performance with large datasets.",
                    ReviewType = "Performance",
                    Severity = "High",
                    Status = "Completed",
                    ReviewedAt = DateTime.Now.AddDays(-27)
                },
                new CodeReview
                {
                    ProjectId = 2,
                    Title = "Best Practices - Error Handling",
                    CodeSnippet = "try {\n    // some code\n} catch (Exception ex) {\n    // empty catch\n}",
                    AiReviewResult = "MEDIUM: Empty catch block detected. Always log exceptions or handle them appropriately. Consider specific exception types instead of generic Exception.",
                    ReviewType = "Best Practices",
                    Severity = "Medium",
                    Status = "In Progress",
                    ReviewedAt = DateTime.Now.AddDays(-22)
                }
            };

            await context.CodeReviews.AddRangeAsync(codeReviews);
            await context.SaveChangesAsync();

            // Add sample bug reports
            var bugReports = new BugReport[]
            {
                new BugReport
                {
                    ProjectId = 1,
                    Title = "Memory Leak in Data Cache",
                    Description = "Application experiencing memory leaks when caching large datasets. AI analysis suggests improper disposal of cached objects.",
                    AiDetectionResult = "AI Detection: Memory leak pattern identified. The cache implementation does not release objects properly. Recommend implementing IDisposable pattern and using WeakReference for cached items.",
                    Severity = "High",
                    Status = "Open",
                    DetectedAt = DateTime.Now.AddDays(-26)
                },
                new BugReport
                {
                    ProjectId = 2,
                    Title = "Race Condition in Async Method",
                    Description = "Multiple threads accessing shared resource causing inconsistent state.",
                    AiDetectionResult = "AI Detection: Race condition detected in asynchronous method. Use SemaphoreSlim or other synchronization primitives to ensure thread-safe access to shared resources.",
                    Severity = "Critical",
                    Status = "In Progress",
                    DetectedAt = DateTime.Now.AddDays(-21)
                },
                new BugReport
                {
                    ProjectId = 3,
                    Title = "Null Reference in Test Generator",
                    Description = "Null reference exception when generating test cases for edge cases.",
                    AiDetectionResult = "AI Detection: Potential null reference at line 145 in TestGenerator.cs. Add null check before accessing object properties or use null-conditional operator.",
                    Severity = "Medium",
                    Status = "Resolved",
                    DetectedAt = DateTime.Now.AddDays(-18),
                    ResolvedAt = DateTime.Now.AddDays(-15)
                }
            };

            await context.BugReports.AddRangeAsync(bugReports);
            await context.SaveChangesAsync();

            // Add sample test cases
            var testCases = new TestCase[]
            {
                new TestCase
                {
                    ProjectId = 1,
                    TestName = "Authentication_Success_Test",
                    TestDescription = "Test successful authentication with valid credentials",
                    AiGeneratedCode = "[Test]\npublic void Authentication_Success() {\n    var result = Authenticate(\"user\", \"pass123\");\n    Assert.IsTrue(result);\n}",
                    TestType = "Unit Test",
                    Status = "Passed",
                    GeneratedAt = DateTime.Now.AddDays(-27),
                    LastRunAt = DateTime.Now.AddDays(-26)
                },
                new TestCase
                {
                    ProjectId = 2,
                    TestName = "BugDetection_Accuracy_Test",
                    TestDescription = "Test accuracy of AI bug detection algorithm",
                    AiGeneratedCode = "[Test]\npublic void BugDetection_Accuracy() {\n    var detector = new BugDetector();\n    var result = detector.AnalyzeCode(testCode);\n    Assert.IsTrue(result.DetectedBugs > 0);\n}",
                    TestType = "Integration Test",
                    Status = "Passed",
                    GeneratedAt = DateTime.Now.AddDays(-23),
                    LastRunAt = DateTime.Now.AddDays(-22)
                },
                new TestCase
                {
                    ProjectId = 3,
                    TestName = "TestCaseGenerator_EdgeCases_Test",
                    TestDescription = "Test generation of edge case test scenarios",
                    AiGeneratedCode = "[Test]\npublic void Generate_EdgeCases() {\n    var generator = new TestGenerator();\n    var tests = generator.GenerateEdgeCases(typeof(MyClass));\n    Assert.IsNotEmpty(tests);\n}",
                    TestType = "Functional Test",
                    Status = "Failed",
                    GeneratedAt = DateTime.Now.AddDays(-19),
                    LastRunAt = DateTime.Now.AddDays(-18)
                }
            };

            await context.TestCases.AddRangeAsync(testCases);
            await context.SaveChangesAsync();

            // Add sample DevOps tasks
            var devOpsTasks = new DevOpsTask[]
            {
                new DevOpsTask
                {
                    ProjectId = 1,
                    TaskName = "Setup CI/CD Pipeline",
                    Description = "Configure automated build and deployment pipeline with AI-powered code review integration",
                    AiSuggestion = "AI Suggestion: Use GitHub Actions with Copilot Chat for automated PR reviews. Configure triggers for pull requests to run AI code analysis before merge.",
                    TaskType = "CI/CD Automation",
                    Status = "Completed",
                    CreatedAt = DateTime.Now.AddDays(-29),
                    CompletedAt = DateTime.Now.AddDays(-27)
                },
                new DevOpsTask
                {
                    ProjectId = 2,
                    TaskName = "PR Summary Automation",
                    Description = "Automate pull request summaries using AI to generate change descriptions",
                    AiSuggestion = "AI Suggestion: Integrate Claude API to analyze PR diffs and generate comprehensive summaries including impacted modules, potential risks, and testing recommendations.",
                    TaskType = "PR Summary",
                    Status = "In Progress",
                    CreatedAt = DateTime.Now.AddDays(-24)
                },
                new DevOpsTask
                {
                    ProjectId = 3,
                    TaskName = "Automated Testing Pipeline",
                    Description = "Setup pipeline for AI-generated test cases with automatic execution",
                    AiSuggestion = "AI Suggestion: Implement test pipeline using Azure DevOps with scheduled runs. Use AI to prioritize test cases based on code changes and historical failure rates.",
                    TaskType = "CI/CD Automation",
                    Status = "Pending",
                    CreatedAt = DateTime.Now.AddDays(-20)
                }
            };

            await context.DevOpsTasks.AddRangeAsync(devOpsTasks);
            await context.SaveChangesAsync();
        }
    }
}
