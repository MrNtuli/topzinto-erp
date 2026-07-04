using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.Contracts;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Infrastructure.Common;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class ContractService : IContractService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public ContractService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IReadOnlyList<ContractDto>> GetAllAsync(string? search = null, string? status = null, CancellationToken ct = default)
    {
        var query = _db.Contracts
            .Include(c => c.Client)
            .Include(c => c.Project)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Title.Contains(search) || c.ContractNumber.Contains(search));

        if (!string.IsNullOrWhiteSpace(status))
        {
            var s = EnumDisplay.ParseContractStatus(status);
            query = query.Where(c => c.Status == s);
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ContractDto(
                c.Id,
                c.ContractNumber,
                c.Title,
                c.Client.Name,
                c.ClientId,
                EnumDisplay.FormatContractStatus(c.Status),
                c.Value,
                c.EndDate.HasValue ? c.EndDate.Value.ToString("dd MMM yyyy") : null,
                c.Project != null ? c.Project.Name : null
            ))
            .ToListAsync(ct);
    }

    public async Task<ContractDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var c = await _db.Contracts
            .Include(x => x.Client)
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return c is null ? null : MapDetail(c);
    }

    public async Task<ContractDetailDto> CreateAsync(CreateContractRequest request, Guid? userId, CancellationToken ct = default)
    {
        var contract = new Contract
        {
            ContractNumber = request.ContractNumber,
            Title = request.Title,
            ClientId = request.ClientId,
            ProjectId = request.ProjectId,
            Value = request.Value,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            RetentionPercent = request.RetentionPercent,
            Status = EnumDisplay.ParseContractStatus(request.Status),
            Notes = request.Notes,
            CreatedBy = userId,
        };

        _db.Contracts.Add(contract);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(contract).Reference(c => c.Client).LoadAsync(ct);
        if (contract.ProjectId.HasValue)
            await _db.Entry(contract).Reference(c => c.Project).LoadAsync(ct);
        await _audit.LogAsync(userId, "", "Create", "Contracts", "Contract", contract.Id.ToString(), newValues: contract.Title, ct: ct);
        return MapDetail(contract);
    }

    public async Task<ContractDetailDto?> UpdateAsync(Guid id, UpdateContractRequest request, Guid? userId, CancellationToken ct = default)
    {
        var contract = await _db.Contracts
            .Include(c => c.Client)
            .Include(c => c.Project)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
        if (contract is null) return null;

        contract.ContractNumber = request.ContractNumber;
        contract.Title = request.Title;
        contract.ClientId = request.ClientId;
        contract.ProjectId = request.ProjectId;
        contract.Value = request.Value;
        contract.StartDate = request.StartDate;
        contract.EndDate = request.EndDate;
        contract.RetentionPercent = request.RetentionPercent;
        contract.Status = EnumDisplay.ParseContractStatus(request.Status);
        contract.Notes = request.Notes;
        contract.UpdatedAt = DateTime.UtcNow;
        contract.UpdatedBy = userId;

        await _db.SaveChangesAsync(ct);
        return MapDetail(contract);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid? userId, CancellationToken ct = default)
    {
        var contract = await _db.Contracts.FindAsync([id], ct);
        if (contract is null) return false;
        contract.IsDeleted = true;
        contract.UpdatedAt = DateTime.UtcNow;
        contract.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static ContractDetailDto MapDetail(Contract c) => new(
        c.Id,
        c.ContractNumber,
        c.Title,
        c.ClientId,
        c.Client.Name,
        c.ProjectId,
        c.Project?.Name,
        c.Value,
        c.StartDate?.ToString("dd MMM yyyy"),
        c.EndDate?.ToString("dd MMM yyyy"),
        c.RetentionPercent,
        EnumDisplay.FormatContractStatus(c.Status),
        c.Notes
    );
}
