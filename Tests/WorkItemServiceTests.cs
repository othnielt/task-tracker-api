using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data;
using TaskTrackerApi.Models;
using TaskTrackerApi.Services;
using Xunit;

namespace TaskTrackerApi.Tests;

/// <summary>
/// Unit tests for WorkItemService using the EF Core in-memory provider,
/// so no real SQL Server instance is needed to run them.
/// </summary>
public class WorkItemServiceTests
{
    private static AppDbContext NewInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_AddsWorkItem()
    {
        using var db = NewInMemoryContext();
        var service = new WorkItemService(db);

        var item = new WorkItem { Reference = "TASK-300", Title = "Test item", AssignedTo = "Tester" };
        var created = await service.CreateAsync(item);

        Assert.True(created.Id > 0);
        Assert.Single(await service.GetAllAsync());
    }

    [Fact]
    public async Task AdvanceStatusAsync_MovesToNextStage()
    {
        using var db = NewInMemoryContext();
        var service = new WorkItemService(db);

        var item = await service.CreateAsync(new WorkItem
        {
            Reference = "TASK-301",
            Title = "Advance test",
            AssignedTo = "Tester",
            Status = WorkItemStatus.ToDo
        });

        var ok = await service.AdvanceStatusAsync(item.Id);
        var updated = await service.GetByIdAsync(item.Id);

        Assert.True(ok);
        Assert.Equal(WorkItemStatus.InProgress, updated!.Status);
    }

    [Fact]
    public async Task DeleteAsync_RemovesWorkItem()
    {
        using var db = NewInMemoryContext();
        var service = new WorkItemService(db);

        var item = await service.CreateAsync(new WorkItem
        {
            Reference = "TASK-302", Title = "Delete test", AssignedTo = "Tester"
        });

        var ok = await service.DeleteAsync(item.Id);

        Assert.True(ok);
        Assert.Empty(await service.GetAllAsync());
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenMissing()
    {
        using var db = NewInMemoryContext();
        var service = new WorkItemService(db);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }
}
