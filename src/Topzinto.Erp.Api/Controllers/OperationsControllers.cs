using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Topzinto.Erp.Application.DTOs.Tasks;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Api.Authorization;

namespace Topzinto.Erp.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/tasks")]
[Authorize(Policy = ErpModules.SchedulePolicy)]
public class ProjectTasksController : ControllerBase
{
    private readonly IProjectTaskService _service;

    public ProjectTasksController(IProjectTaskService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetByProject(Guid projectId, CancellationToken ct) =>
        Ok(await _service.GetByProjectAsync(projectId, ct));

    [HttpPost]
    public async Task<IActionResult> Create(Guid projectId, [FromBody] CreateProjectTaskRequest request, CancellationToken ct)
    {
        if (request.ProjectId != projectId)
            return BadRequest(new { message = "Project ID mismatch" });
        var userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (Guid?)null;
        return Ok(await _service.CreateAsync(request, userId, ct));
    }
}

[ApiController]
[Route("api/tasks")]
[Authorize(Policy = ErpModules.SchedulePolicy)]
public class TasksController : ControllerBase
{
    private readonly IProjectTaskService _service;

    public TasksController(IProjectTaskService service) => _service = service;

    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdue(CancellationToken ct) =>
        Ok(await _service.GetOverdueAsync(ct));
}

[ApiController]
[Route("api/projects/{projectId:guid}/milestones")]
[Authorize(Policy = ErpModules.SchedulePolicy)]
public class ProjectMilestonesController : ControllerBase
{
    private readonly IProjectMilestoneService _service;

    public ProjectMilestonesController(IProjectMilestoneService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetByProject(Guid projectId, CancellationToken ct) =>
        Ok(await _service.GetByProjectAsync(projectId, ct));

    [HttpPost]
    public async Task<IActionResult> Create(Guid projectId, [FromBody] CreateMilestoneRequest request, CancellationToken ct)
    {
        if (request.ProjectId != projectId)
            return BadRequest(new { message = "Project ID mismatch" });
        var userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (Guid?)null;
        return Ok(await _service.CreateAsync(request, userId, ct));
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ErpModules.SchedulePolicy)]
public class ScheduleController : ControllerBase
{
    private readonly IScheduleService _service;

    public ScheduleController(IScheduleService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetEvents([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct) =>
        Ok(await _service.GetEventsAsync(from, to, ct));

    [HttpGet("gantt")]
    public async Task<IActionResult> GetGantt([FromQuery] Guid? projectId, CancellationToken ct) =>
        Ok(await _service.GetGanttDataAsync(projectId, ct));
}
