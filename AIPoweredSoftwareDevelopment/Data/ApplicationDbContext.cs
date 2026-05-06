using Microsoft.EntityFrameworkCore;
using AIPoweredSoftwareDevelopment.Models;

namespace AIPoweredSoftwareDevelopment.Data
{
    /// <summary>
    /// Application database context for SQLite database
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Projects table - stores project information
        /// </summary>
        public DbSet<Project> Projects { get; set; }
        
        /// <summary>
        /// CodeReviews table - stores AI code review results
        /// </summary>
        public DbSet<CodeReview> CodeReviews { get; set; }
        
        /// <summary>
        /// BugReports table - stores bug detection results
        /// </summary>
        public DbSet<BugReport> BugReports { get; set; }
        
        /// <summary>
        /// TestCases table - stores AI-generated test cases
        /// </summary>
        public DbSet<TestCase> TestCases { get; set; }
        
        /// <summary>
        /// DevOpsTasks table - stores DevOps task suggestions
        /// </summary>
        public DbSet<DevOpsTask> DevOpsTasks { get; set; }
        
        /// <summary>
        /// ErrorLogs table - stores application error logs
        /// </summary>
        public DbSet<ErrorLog> ErrorLogs { get; set; }

        /// <summary>
        /// Configures model relationships and delete behavior
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Project -> CodeReviews relationship (cascade delete)
            modelBuilder.Entity<Project>()
                .HasMany(p => p.CodeReviews)
                .WithOne(c => c.Project)
                .HasForeignKey(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Project -> BugReports relationship (cascade delete)
            modelBuilder.Entity<Project>()
                .HasMany(p => p.BugReports)
                .WithOne(b => b.Project)
                .HasForeignKey(b => b.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Project -> TestCases relationship (cascade delete)
            modelBuilder.Entity<Project>()
                .HasMany(p => p.TestCases)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Project -> DevOpsTasks relationship (cascade delete)
            modelBuilder.Entity<Project>()
                .HasMany(p => p.DevOpsTasks)
                .WithOne(d => d.Project)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
