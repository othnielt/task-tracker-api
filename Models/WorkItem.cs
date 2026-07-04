using System.ComponentModel.DataAnnotations;

namespace TaskTrackerApi.Models;

/// <summary>
/// Represents a work item (task) tracked through a simple workflow.
/// </summary>
public class WorkItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Reference code is required.")]
    [StringLength(20, ErrorMessage = "Reference code cannot exceed 20 characters.")]
    public string Reference { get; set; } = string.Empty;   // e.g. "TASK-101"

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(300, ErrorMessage = "Title cannot exceed 300 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public WorkItemStatus Status { get; set; } = WorkItemStatus.ToDo;

    [Required]
    public string AssignedTo { get; set; } = string.Empty;

    public WorkItemPriority Priority { get; set; } = WorkItemPriority.Medium;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdated { get; set; }
}

/// <summary>The workflow stages a work item moves through.</summary>
public enum WorkItemStatus
{
    ToDo,
    InProgress,
    Review,
    Done,
    Cancelled
}

/// <summary>Priority level for a work item.</summary>
public enum WorkItemPriority
{
    Low,
    Medium,
    High,
    Urgent
}
