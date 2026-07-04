using Topzinto.Erp.Application.DTOs.Tasks;

namespace Topzinto.Erp.Application.Interfaces;

public interface IProjectTaskService
{
    Task<IReadOnlyList<ProjectTaskDto>> GetByProjectAsync(Guid projectId, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectTaskDto>> GetOverdueAsync(CancellationToken ct = default);
    Task<ProjectTaskDto> CreateAsync(CreateProjectTaskRequest request, Guid? userId, CancellationToken ct = default);
}

public interface IProjectMilestoneService
{
    Task<IReadOnlyList<ProjectMilestoneDto>> GetByProjectAsync(Guid projectId, CancellationToken ct = default);
    Task<ProjectMilestoneDto> CreateAsync(CreateMilestoneRequest request, Guid? userId, CancellationToken ct = default);
}

public interface IScheduleService
{
    Task<IReadOnlyList<ScheduleEventDto>> GetEventsAsync(DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
}
