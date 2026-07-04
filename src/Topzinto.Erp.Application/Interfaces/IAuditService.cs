using Topzinto.Erp.Domain.Entities;

namespace Topzinto.Erp.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        Guid? userId,
        string userEmail,
        string action,
        string module,
        string entityType,
        string entityId,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count = 50, CancellationToken ct = default);
}
