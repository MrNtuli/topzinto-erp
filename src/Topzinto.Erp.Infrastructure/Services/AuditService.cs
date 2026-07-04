using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;

    public AuditService(AppDbContext context) => _context = context;

    public async Task LogAsync(
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
        CancellationToken ct = default)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            UserEmail = userEmail,
            Action = action,
            Module = module,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            UserAgent = userAgent,
        });

        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count = 50, CancellationToken ct = default) =>
        await _context.AuditLogs
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .ToListAsync(ct);
}
