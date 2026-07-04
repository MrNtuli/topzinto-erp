using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Infrastructure.Identity;

namespace Topzinto.Erp.Infrastructure.Caching;

public class DashboardCacheInvalidationInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<Type> ExcludedTypes =
    [
        typeof(AuditLog),
        typeof(Notification),
        typeof(CompanySetting),
        typeof(ChatChannel),
        typeof(ChatMessage),
        typeof(ChatChannelRead),
        typeof(ChatChannelMember),
        typeof(ApplicationUser),
    ];

    private readonly IAppCacheInvalidator _invalidator;
    private bool _shouldInvalidate;

    public DashboardCacheInvalidationInterceptor(IAppCacheInvalidator invalidator) =>
        _invalidator = invalidator;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        _shouldInvalidate = HasRelevantChanges(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        _shouldInvalidate = HasRelevantChanges(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        InvalidateIfNeeded(result, CancellationToken.None).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await InvalidateIfNeeded(result, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task InvalidateIfNeeded(int result, CancellationToken ct)
    {
        if (_shouldInvalidate && result > 0)
            await _invalidator.InvalidateDashboardAndReportsAsync(ct);
        _shouldInvalidate = false;
    }

    private static bool HasRelevantChanges(DbContext? context)
    {
        if (context is null) return false;

        return context.ChangeTracker.Entries().Any(entry =>
            entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
            && !ExcludedTypes.Contains(entry.Entity.GetType()));
    }
}
