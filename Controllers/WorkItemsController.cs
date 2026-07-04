using Microsoft.AspNetCore.Mvc;
using TaskTrackerApi.Models;
using TaskTrackerApi.Services;

namespace TaskTrackerApi.Controllers;

/// <summary>
/// REST API for tracking work items. Exposes standard CRUD endpoints plus a
/// domain-specific "advance" action. This is the presentation layer; it delegates
/// all logic to the service layer.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WorkItemsController : ControllerBase
{
    private readonly IWorkItemService _service;

    public WorkItemsController(IWorkItemService service) => _service = service;

    /// <summary>Get all work items.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkItem>>> GetAll() =>
        Ok(await _service.GetAllAsync());

    /// <summary>Get a single work item by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkItem>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Create a new work item.</summary>
    [HttpPost]
    public async Task<ActionResult<WorkItem>> Create([FromBody] WorkItem item)
    {
        // ModelState validation runs automatically thanks to [ApiController].
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _service.CreateAsync(item);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Update an existing work item.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] WorkItem item)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var ok = await _service.UpdateAsync(id, item);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Advance a work item to the next workflow stage.</summary>
    [HttpPost("{id:int}/advance")]
    public async Task<IActionResult> Advance(int id)
    {
        var ok = await _service.AdvanceStatusAsync(id);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Delete a work item.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }
}
