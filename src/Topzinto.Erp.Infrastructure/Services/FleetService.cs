using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.Fleet;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Domain.Enums;
using Topzinto.Erp.Infrastructure.Common;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class FleetService : IFleetService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public FleetService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<FleetSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var vehicles = await _db.Vehicles.ToListAsync(ct);
        var soon = DateTime.UtcNow.Date.AddDays(30);
        return new FleetSummaryDto(
            vehicles.Count,
            vehicles.Count(v => v.Status == VehicleStatus.Available),
            vehicles.Count(v => v.Status == VehicleStatus.InUse),
            vehicles.Count(v => v.Status == VehicleStatus.Maintenance),
            vehicles.Count(v =>
                (v.LicenseExpiryDate <= soon) || (v.InsuranceExpiryDate <= soon) || (v.RoadworthyExpiryDate <= soon))
        );
    }

    public async Task<IReadOnlyList<VehicleDto>> GetAllAsync(string? search = null, string? status = null, CancellationToken ct = default)
    {
        var query = _db.Vehicles.Include(v => v.AssignedProject).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(v => v.RegistrationNumber.Contains(search) || v.MakeModel.Contains(search) || (v.DriverName != null && v.DriverName.Contains(search)));

        if (!string.IsNullOrWhiteSpace(status))
        {
            var s = AssetDisplay.ParseVehicleStatus(status);
            query = query.Where(v => v.Status == s);
        }

        var list = await query.OrderBy(v => v.RegistrationNumber).ToListAsync(ct);
        return list.Select(MapList).ToList();
    }

    public async Task<VehicleDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var v = await _db.Vehicles
            .Include(x => x.AssignedProject)
            .Include(x => x.MaintenanceRecords)
            .Include(x => x.FuelLogs)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return v is null ? null : MapDetail(v);
    }

    public async Task<VehicleDetailDto> CreateAsync(CreateVehicleRequest request, Guid? userId, CancellationToken ct = default)
    {
        var vehicle = MapToEntity(new Vehicle(), request);
        vehicle.CreatedBy = userId;
        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Create", "Fleet", "Vehicle", vehicle.Id.ToString(), newValues: vehicle.RegistrationNumber, ct: ct);
        return (await GetByIdAsync(vehicle.Id, ct))!;
    }

    public async Task<VehicleDetailDto?> UpdateAsync(Guid id, UpdateVehicleRequest request, Guid? userId, CancellationToken ct = default)
    {
        var vehicle = await _db.Vehicles.FindAsync([id], ct);
        if (vehicle is null) return null;
        MapToEntity(vehicle, request);
        vehicle.UpdatedAt = DateTime.UtcNow;
        vehicle.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    private static Vehicle MapToEntity(Vehicle v, CreateVehicleRequest r)
    {
        v.RegistrationNumber = r.RegistrationNumber;
        v.MakeModel = r.MakeModel;
        v.Type = AssetDisplay.ParseVehicleType(r.Type);
        v.Status = AssetDisplay.ParseVehicleStatus(r.Status);
        v.DriverName = r.DriverName;
        v.DriverLicenseNumber = r.DriverLicenseNumber;
        v.LicenseExpiryDate = r.LicenseExpiryDate;
        v.InsuranceExpiryDate = r.InsuranceExpiryDate;
        v.RoadworthyExpiryDate = r.RoadworthyExpiryDate;
        v.CurrentLocation = r.CurrentLocation;
        v.AssignedProjectId = r.AssignedProjectId;
        v.Notes = r.Notes;
        return v;
    }

    private static Vehicle MapToEntity(Vehicle v, UpdateVehicleRequest r) =>
        MapToEntity(v, new CreateVehicleRequest(
            r.RegistrationNumber, r.MakeModel, r.Type, r.Status, r.DriverName, r.DriverLicenseNumber,
            r.LicenseExpiryDate, r.InsuranceExpiryDate, r.RoadworthyExpiryDate, r.CurrentLocation,
            r.AssignedProjectId, r.Notes));

    private static VehicleDto MapList(Vehicle v) => new(
        v.Id, v.RegistrationNumber, v.MakeModel,
        AssetDisplay.FormatVehicleType(v.Type),
        AssetDisplay.FormatVehicleStatus(v.Status),
        v.DriverName, v.CurrentLocation,
        v.AssignedProject?.Name,
        v.LicenseExpiryDate?.ToString("dd MMM yyyy"),
        AssetDisplay.IsExpiringSoon(v.LicenseExpiryDate) || AssetDisplay.IsExpiringSoon(v.InsuranceExpiryDate)
    );

    private static VehicleDetailDto MapDetail(Vehicle v) => new(
        v.Id, v.RegistrationNumber, v.MakeModel,
        AssetDisplay.FormatVehicleType(v.Type),
        AssetDisplay.FormatVehicleStatus(v.Status),
        v.DriverName, v.DriverLicenseNumber,
        v.LicenseExpiryDate?.ToString("dd MMM yyyy"),
        v.InsuranceExpiryDate?.ToString("dd MMM yyyy"),
        v.RoadworthyExpiryDate?.ToString("dd MMM yyyy"),
        v.CurrentLocation, v.AssignedProjectId, v.AssignedProject?.Name, v.Notes,
        v.MaintenanceRecords.OrderByDescending(m => m.ServiceDate).Select(m => new MaintenanceDto(
            m.Id, m.ServiceDate.ToString("dd MMM yyyy"), m.Description, m.Cost,
            m.NextServiceDue?.ToString("dd MMM yyyy"), m.ServiceProvider)).ToList(),
        v.FuelLogs.OrderByDescending(f => f.LogDate).Select(f => new FuelLogDto(
            f.Id, f.LogDate.ToString("dd MMM yyyy"), f.Litres, f.Cost, f.OdometerReading, f.Notes)).ToList(),
        v.LicenseExpiryDate?.ToString("yyyy-MM-dd"),
        v.InsuranceExpiryDate?.ToString("yyyy-MM-dd"),
        v.RoadworthyExpiryDate?.ToString("yyyy-MM-dd")
    );
}
