using Topzinto.Erp.Application.DTOs.Projects;

namespace Topzinto.Erp.Application.Interfaces;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectDto>> GetAllAsync(string? search = null, string? status = null, Guid? clientId = null, CancellationToken ct = default);
    Task<ProjectDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProjectDetailDto> CreateAsync(CreateProjectRequest request, Guid? userId, CancellationToken ct = default);
    Task<ProjectDetailDto?> UpdateAsync(Guid id, UpdateProjectRequest request, Guid? userId, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, Guid? userId, CancellationToken ct = default);
}
