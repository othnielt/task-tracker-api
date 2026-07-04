using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Services;

/// <summary>
/// Business-logic contract for managing work items. Keeping this behind an
/// interface separates the business layer from the presentation (controller)
/// layer and makes the code easy to unit test.
/// </summary>
public interface IWorkItemService
{
    Task<IEnumerable<WorkItem>> GetAllAsync();
    Task<WorkItem?> GetByIdAsync(int id);
    Task<WorkItem> CreateAsync(WorkItem item);
    Task<bool> UpdateAsync(int id, WorkItem item);
    Task<bool> DeleteAsync(int id);
    Task<bool> AdvanceStatusAsync(int id);
}

public class WorkItemService : IWorkItemService
{
    private readonly AppDbContext _db;

    public WorkItemService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<WorkItem>> GetAllAsync() =>
        await _db.WorkItems.OrderBy(w => w.Reference).ToListAsync();

    public async Task<WorkItem?> GetByIdAsync(int id) =>
        await _db.WorkItems.FindAsync(id);

    public async Task<WorkItem> CreateAsync(WorkItem item)
    {
        item.CreatedDate = item.CreatedDate == default ? DateTime.UtcNow : item.CreatedDate;
        item.LastUpdated = DateTime.UtcNow;
        _db.WorkItems.Add(item);
        await _db.SaveChangesAsync();
        return item;
    }

    public async Task<bool> UpdateAsync(int id, WorkItem updated)
    {
        var existing = await _db.WorkItems.FindAsync(id);
        if (existing is null) return false;

        existing.Reference = updated.Reference;
        existing.Title = updated.Title;
        existing.Description = updated.Description;
        existing.Status = updated.Status;
        existing.AssignedTo = updated.AssignedTo;
        existing.Priority = updated.Priority;
        existing.LastUpdated = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _db.WorkItems.FindAsync(id);
        if (existing is null) return false;

        _db.WorkItems.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Moves a work item to the next stage in the workflow.
    /// Demonstrates a small piece of real business logic.
    /// </summary>
    public async Task<bool> AdvanceStatusAsync(int id)
    {
        var item = await _db.WorkItems.FindAsync(id);
        if (item is null) return false;

        item.Status = item.Status switch
        {
            WorkItemStatus.ToDo => WorkItemStatus.InProgress,
            WorkItemStatus.InProgress => WorkItemStatus.Review,
            WorkItemStatus.Review => WorkItemStatus.Done,
            _ => item.Status   // Done / Cancelled are terminal states
        };
        item.LastUpdated = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }
}
