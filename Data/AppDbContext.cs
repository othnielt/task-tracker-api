using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Data;

/// <summary>
/// Entity Framework Core context that maps the domain model to Microsoft SQL Server.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<WorkItem> WorkItems => Set<WorkItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Store enums as readable strings in the database rather than ints.
        modelBuilder.Entity<WorkItem>()
            .Property(w => w.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<WorkItem>()
            .Property(w => w.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Enforce a unique reference code at the database level.
        modelBuilder.Entity<WorkItem>()
            .HasIndex(w => w.Reference)
            .IsUnique();

        // Seed a couple of sample rows so the app has data on first run.
        modelBuilder.Entity<WorkItem>().HasData(
            new WorkItem
            {
                Id = 1,
                Reference = "TASK-101",
                Title = "Set up CI pipeline",
                Description = "Configure automated build and test on each push.",
                Status = WorkItemStatus.InProgress,
                AssignedTo = "A. Developer",
                Priority = WorkItemPriority.High,
                CreatedDate = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc)
            },
            new WorkItem
            {
                Id = 2,
                Reference = "TASK-102",
                Title = "Add input validation to API",
                Description = "Validate required fields and length limits on all endpoints.",
                Status = WorkItemStatus.Review,
                AssignedTo = "B. Engineer",
                Priority = WorkItemPriority.Medium,
                CreatedDate = new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
