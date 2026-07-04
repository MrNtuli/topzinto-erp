using Topzinto.Erp.Domain.Common;
using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Domain.Entities;

public class Vehicle : BaseEntity
{
    public string RegistrationNumber { get; set; } = string.Empty;
    public string MakeModel { get; set; } = string.Empty;
    public VehicleType Type { get; set; } = VehicleType.LDV;
    public VehicleStatus Status { get; set; } = VehicleStatus.Available;
    public string? DriverName { get; set; }
    public string? DriverLicenseNumber { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }
    public DateTime? RoadworthyExpiryDate { get; set; }
    public string? CurrentLocation { get; set; }
    public Guid? AssignedProjectId { get; set; }
    public Project? AssignedProject { get; set; }
    public string? Notes { get; set; }

    public ICollection<VehicleMaintenance> MaintenanceRecords { get; set; } = [];
    public ICollection<FuelLog> FuelLogs { get; set; } = [];
}

public class VehicleMaintenance : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    public DateTime ServiceDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public DateTime? NextServiceDue { get; set; }
    public string? ServiceProvider { get; set; }
}

public class FuelLog : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    public DateTime LogDate { get; set; }
    public decimal Litres { get; set; }
    public decimal Cost { get; set; }
    public decimal? OdometerReading { get; set; }
    public Guid? ProjectId { get; set; }
    public string? Notes { get; set; }
}

public class Equipment : BaseEntity
{
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public EquipmentCategory Category { get; set; } = EquipmentCategory.Other;
    public EquipmentStatus Status { get; set; } = EquipmentStatus.Available;
    public string? MakeModel { get; set; }
    public string? SerialNumber { get; set; }
    public string? OperatorName { get; set; }
    public DateTime? LastInspectionDate { get; set; }
    public DateTime? NextInspectionDue { get; set; }
    public DateTime? LastServiceDate { get; set; }
    public DateTime? NextServiceDue { get; set; }
    public Guid? AssignedProjectId { get; set; }
    public Project? AssignedProject { get; set; }
    public string? Notes { get; set; }

    public ICollection<EquipmentBooking> Bookings { get; set; } = [];
    public ICollection<EquipmentInspection> Inspections { get; set; } = [];
}

public class EquipmentBooking : BaseEntity
{
    public Guid EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? BookedByName { get; set; }
    public string? Notes { get; set; }
}

public class EquipmentInspection : BaseEntity
{
    public Guid EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;
    public DateTime InspectionDate { get; set; }
    public string Result { get; set; } = "Pass";
    public string? InspectorName { get; set; }
    public string? Notes { get; set; }
    public DateTime? NextDueDate { get; set; }
}
