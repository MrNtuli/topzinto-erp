namespace Topzinto.Erp.Application.DTOs.Fleet;

public record FleetSummaryDto(int Total, int Available, int InUse, int Maintenance, int ExpiringSoon);

public record VehicleDto(
    Guid Id,
    string RegistrationNumber,
    string MakeModel,
    string Type,
    string Status,
    string? DriverName,
    string? CurrentLocation,
    string? AssignedProjectName,
    string? LicenseExpiryDate,
    bool IsExpiringSoon
);

public record VehicleDetailDto(
    Guid Id,
    string RegistrationNumber,
    string MakeModel,
    string Type,
    string Status,
    string? DriverName,
    string? DriverLicenseNumber,
    string? LicenseExpiryDate,
    string? InsuranceExpiryDate,
    string? RoadworthyExpiryDate,
    string? CurrentLocation,
    Guid? AssignedProjectId,
    string? AssignedProjectName,
    string? Notes,
    IReadOnlyList<MaintenanceDto> MaintenanceRecords,
    IReadOnlyList<FuelLogDto> FuelLogs,
    string? LicenseExpiryInput,
    string? InsuranceExpiryInput,
    string? RoadworthyExpiryInput
);

public record MaintenanceDto(Guid Id, string ServiceDate, string Description, decimal Cost, string? NextServiceDue, string? ServiceProvider);
public record FuelLogDto(Guid Id, string LogDate, decimal Litres, decimal Cost, decimal? OdometerReading, string? Notes);

public record CreateVehicleRequest(
    string RegistrationNumber,
    string MakeModel,
    string Type,
    string Status,
    string? DriverName,
    string? DriverLicenseNumber,
    DateTime? LicenseExpiryDate,
    DateTime? InsuranceExpiryDate,
    DateTime? RoadworthyExpiryDate,
    string? CurrentLocation,
    Guid? AssignedProjectId,
    string? Notes
);

public record UpdateVehicleRequest(
    string RegistrationNumber,
    string MakeModel,
    string Type,
    string Status,
    string? DriverName,
    string? DriverLicenseNumber,
    DateTime? LicenseExpiryDate,
    DateTime? InsuranceExpiryDate,
    DateTime? RoadworthyExpiryDate,
    string? CurrentLocation,
    Guid? AssignedProjectId,
    string? Notes
);

public record CreateFuelLogRequest(
    DateTime LogDate,
    decimal Litres,
    decimal Cost,
    decimal? OdometerReading,
    Guid? ProjectId,
    string? Notes
);
