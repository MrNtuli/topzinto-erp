using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.Clients;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Infrastructure.Common;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public ClientService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IReadOnlyList<ClientDto>> GetAllAsync(string? search = null, CancellationToken ct = default)
    {
        var query = _db.Clients
            .Include(c => c.Contacts)
            .Include(c => c.Projects)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search));

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new ClientDto(
                c.Id,
                c.Name,
                EnumDisplay.FormatClientType(c.Type),
                c.City,
                c.Province,
                c.Projects.Count(p => !p.IsDeleted),
                c.Contacts.Where(x => x.IsPrimary).Select(x => x.Name).FirstOrDefault()
                    ?? c.Contacts.Select(x => x.Name).FirstOrDefault()
            ))
            .ToListAsync(ct);
    }

    public async Task<ClientDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var client = await _db.Clients
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
        return client is null ? null : MapDetail(client);
    }

    public async Task<ClientDetailDto> CreateAsync(CreateClientRequest request, Guid? userId, CancellationToken ct = default)
    {
        var client = new Client
        {
            Name = request.Name,
            Type = EnumDisplay.ParseClientType(request.Type),
            RegistrationNumber = request.RegistrationNumber,
            Address = request.Address,
            City = request.City,
            Province = request.Province,
            Notes = request.Notes,
            CreatedBy = userId,
        };

        if (request.Contacts is not null)
        {
            foreach (var c in request.Contacts)
            {
                client.Contacts.Add(new ClientContact
                {
                    Name = c.Name,
                    Title = c.Title,
                    Phone = c.Phone,
                    Email = c.Email,
                    IsPrimary = c.IsPrimary,
                    CreatedBy = userId,
                });
            }
        }

        _db.Clients.Add(client);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Create", "Clients", "Client", client.Id.ToString(), newValues: client.Name, ct: ct);
        return MapDetail(client);
    }

    public async Task<ClientDetailDto?> UpdateAsync(Guid id, UpdateClientRequest request, Guid? userId, CancellationToken ct = default)
    {
        var client = await _db.Clients.Include(c => c.Contacts).FirstOrDefaultAsync(c => c.Id == id, ct);
        if (client is null) return null;

        client.Name = request.Name;
        client.Type = EnumDisplay.ParseClientType(request.Type);
        client.RegistrationNumber = request.RegistrationNumber;
        client.Address = request.Address;
        client.City = request.City;
        client.Province = request.Province;
        client.Notes = request.Notes;
        client.UpdatedAt = DateTime.UtcNow;
        client.UpdatedBy = userId;

        await _db.SaveChangesAsync(ct);
        return MapDetail(client);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid? userId, CancellationToken ct = default)
    {
        var client = await _db.Clients.FindAsync([id], ct);
        if (client is null) return false;
        client.IsDeleted = true;
        client.UpdatedAt = DateTime.UtcNow;
        client.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static ClientDetailDto MapDetail(Client c) => new(
        c.Id,
        c.Name,
        EnumDisplay.FormatClientType(c.Type),
        c.RegistrationNumber,
        c.Address,
        c.City,
        c.Province,
        c.Notes,
        c.Contacts.Select(x => new ClientContactDto(x.Id, x.Name, x.Title, x.Phone, x.Email, x.IsPrimary)).ToList()
    );
}
