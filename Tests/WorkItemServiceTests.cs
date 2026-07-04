using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data;
using TaskTrackerApi.Models;
using TaskTrackerApi.Services;
using Xunit;

namespace TaskTrackerApi.Tests;

/// <summary>
/// Unit tests for <see cref="WorkItemService"/>.
///
/// Each test runs against a fresh EF Core in-memory database (unique name per
/// test), so tests are fully isolated and need no real SQL Server instance.
/// The in-memory provider is not seeded here, so every context starts empty.
/// </summary>
public class WorkItemServiceTests
{
    /// <summary>Creates an isolated in-memory context for a single test.</summary>
    private static AppDbContext NewInMemoryContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options);

    private static WorkItem NewItem(
        string reference = "TASK-100",
        string title = "Sample item",
        string assignedTo = "Tester",
        WorkItemStatus status = WorkItemStatus.ToDo,
        WorkItemPriority priority = WorkItemPriority.Medium) =>
        new()
        {
            Reference = reference,
            Title = title,
            AssignedTo = assignedTo,
            Status = status,
            Priority = priority
        };

    // ---------- CreateAsync ----------

    [Fact]
    public async Task CreateAsync_PersistsItemAndAssignsId()
    {
        using var db = NewInMemoryContext();
        var service = new WorkItemService(db);

        var created = await service.CreateAsync(NewItem(reference: "TASK-300"));

        Assert.True(created.Id > 0);
        Assert.Single(await service.GetAllAsync());
    }

    [Fact]
    public async Task CreateAsync_SetsCreatedDate_WhenNotProvided()
    {
        using var db = NewInMemoryContext();
        var service = new WorkItemService(db);

        var before = DateTime.UtcNow;
        var created = await service.CreateAsync(new WorkItem
        {
            Reference = "TASK-301",
            Title = "No date",
            AssignedTo = "Tester"
            // CreatedDate deliberately left at default(DateTime)
        });

        Assert.InRange(created.CreatedDate, before, DateTime.UtcNow);
        Assert.NotNull(created.LastUpdated);
    }

    [Fact]
    public async Task CreateAsync_PreservesCreatedDate_WhenProvided()
    {
        using var db = NewInMemoryContext();
        var service = new WorkItemService(db);

        var supplied = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var created = await service.CreateAsync(new WorkItem
        {
            Reference = "TASK-302",
            Title = "Explicit date",
            AssignedTo = "Tester",
            CreatedDate = supplied
        });

        Assert.Equal(supplied, created.CreatedDate);
    }

    // ---------- GetAllAsync ----------

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenNoItems()
    {
        using var db = NewInMemoryContext();
        var service = new WorkItemService(db);

        Assert.Empty(await service.GetAllAsync());
    }

    [Fact]
    public async Task GetAllAsync_OrdersByReference()
    {
        using var db = NewInMemoryContext();
        var service = new WorkItemService(db);

        await service.CreateAsync(NewItem(reference: "TASK-300"));
        await service.CreateAsync(NewItem(reference: "TASK-100"));
        await service.CreateAsync(NewItem(reference: "TASK-200"));

        var references = (await service.GetAllAsync()).Select(w => w.Reference).ToList();

        Assert.Equal(new[] { "TASK-100", "TASK-200", "TASK-300" }, references);
    }

    // ---------- GetByIdAsync ----------

    [Fact]
    public async Task GetByIdAsync_ReturnsItem_WhenExists()
    {
        using var db = NewInMemoryContext();
        var service = new WorkItemService(db);

        var created = await service.CreateAsync(NewItem(reference: "TASK-303"));
        var found = await service.GetByIdAsync(created.Id);

        Assert.NotNull(found);
        Assert.Equal("TASK-303", found!.Reference);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenMissing()
    {
        using var db = NewInMemoryContext();
        var service = new WorkItemService(db);

        Assert.Null(await service.GetByIdAsync(999));
    }

    // ---------- UpdateAsync ----------

    [Fact]
    public async Task UpdateAsync_ModifiesFieldsAndRefreshesLastUpdated()
    {
        using var db = NewInMemoryContext();
        var service = new WorkItemService(db);

        var created = await service.CreateAsync(NewItem(reference: "TASK-304"));

        var ok = await service.UpdateAsync(created.Id, new WorkItem
        {
            Reference = "TASK-304",
            Title = "Updated title",
            Description = "Updated description",
            Status = WorkItemStatus.Review,
            AssignedTo = "New Owner",
            Priority = WorkItemPriority.Urgent
        });

        var updated = await service.GetByIdAsync(created.Id);

        Assert.True(ok);
        Assert.NotNull(updated);
        Assert.Equal("Updated title", updated!.Title);
        Assert.Equal("Updated description", updated.Description);
        Assert.Equal(WorkItemStatus.Review, updated.Status);
        Assert.Equal("New Owner", updated.AssignedTo);
        Assert.Equal(WorkItemPriority.Urgent, updated.Priority);
        Assert.NotNull(updated.LastUpdated);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenMissing()
    {
        using var db = NewInMemoryContext();
        var service = new WorkItemService(db);

        var ok = await service.UpdateAsync(999, NewItem());

        Assert.False(ok);
    }

    // ---------- DeleteAsync ----------

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        using var db = NewInMemoryContext();
        var service = new WorkItemService(db);

        var created = await service.CreateAsync(NewItem(reference: "TASK-305"));

        var ok = await service.DeleteAsync(created.Id);

        Assert.True(ok);
        Assert.Empty(await service.GetAllAsync());
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenMissing()
    {
        using var db = NewInMemoryContext();
        var service = new WorkItemService(db);

        Assert.False(await service.DeleteAsync(999));
    }

    // ---------- AdvanceStatusAsync ----------

    [Theory]
    [InlineData(WorkItemStatus.ToDo, WorkItemStatus.InProgress)]
    [InlineData(WorkItemStatus.InProgress, WorkItemStatus.Review)]
    [InlineData(WorkItemStatus.Review, WorkItemStatus.Done)]
    public async Task AdvanceStatusAsync_MovesToNextStage(
        WorkItemStatus from, WorkItemStatus expected)
    {
        using var db = NewInMemoryContext();
        var service = new WorkItemService(db);

        var created = await service.CreateAsync(NewItem(reference: "TASK-306", status: from));

        var ok = await service.AdvanceStatusAsync(created.Id);
        var updated = await service.GetByIdAsync(created.Id);

        Assert.True(ok);
        Assert.Equal(expected, updated!.Status);
    }

    [Theory]
    [InlineData(WorkItemStatus.Done)]
    [InlineData(WorkItemStatus.Cancelled)]
    public async Task AdvanceStatusAsync_LeavesTerminalStatesUnchanged(WorkItemStatus terminal)
    {
        using var db = NewInMemoryContext();
        var service = new WorkItemService(db);

        var created = await service.CreateAsync(NewItem(reference: "TASK-307", status: terminal));

        var ok = await service.AdvanceStatusAsync(created.Id);
        var updated = await service.GetByIdAsync(created.Id);

        Assert.True(ok);
        Assert.Equal(terminal, updated!.Status);
    }

    [Fact]
    public async Task AdvanceStatusAsync_ReturnsFalse_WhenMissing()
    {
        using var db = NewInMemoryContext();
        var service = new WorkItemService(db);

        Assert.False(await service.AdvanceStatusAsync(999));
    }
}
