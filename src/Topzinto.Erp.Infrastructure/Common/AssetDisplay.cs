using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Infrastructure.Common;

public static class AssetDisplay
{
    public static string FormatVehicleStatus(VehicleStatus s) => s switch
    {
        VehicleStatus.Available => "Available",
        VehicleStatus.InUse => "In Use",
        VehicleStatus.Maintenance => "Maintenance",
        VehicleStatus.Decommissioned => "Decommissioned",
        _ => s.ToString(),
    };

    public static VehicleStatus ParseVehicleStatus(string? v) => v?.ToLowerInvariant() switch
    {
        "available" => VehicleStatus.Available,
        "in use" or "inuse" => VehicleStatus.InUse,
        "maintenance" => VehicleStatus.Maintenance,
        "decommissioned" => VehicleStatus.Decommissioned,
        _ => VehicleStatus.Available,
    };

    public static string FormatVehicleType(VehicleType t) => t switch
    {
        VehicleType.LDV => "LDV",
        VehicleType.Bakkie => "Bakkie",
        VehicleType.Truck => "Truck",
        VehicleType.TipperTruck => "Tipper Truck",
        VehicleType.TLB => "TLB",
        VehicleType.WaterTanker => "Water Tanker",
        VehicleType.HeavyVehicle => "Heavy Vehicle",
        _ => "Other",
    };

    public static VehicleType ParseVehicleType(string? v) => v?.ToLowerInvariant() switch
    {
        "ldv" => VehicleType.LDV,
        "bakkie" => VehicleType.Bakkie,
        "truck" => VehicleType.Truck,
        "tipper truck" or "tipper" => VehicleType.TipperTruck,
        "tlb" => VehicleType.TLB,
        "water tanker" => VehicleType.WaterTanker,
        "heavy vehicle" => VehicleType.HeavyVehicle,
        _ => VehicleType.Other,
    };

    public static string FormatEquipmentStatus(EquipmentStatus s) => FormatVehicleStatus((VehicleStatus)(int)s);

    public static EquipmentStatus ParseEquipmentStatus(string? v) =>
        (EquipmentStatus)(int)ParseVehicleStatus(v);

    public static string FormatEquipmentCategory(EquipmentCategory c) => c switch
    {
        EquipmentCategory.Excavator => "Excavator",
        EquipmentCategory.TLB => "TLB",
        EquipmentCategory.Grader => "Grader",
        EquipmentCategory.Roller => "Roller",
        EquipmentCategory.Generator => "Generator",
        EquipmentCategory.Compressor => "Compressor",
        EquipmentCategory.Scaffolding => "Scaffolding",
        EquipmentCategory.PowerTool => "Power Tool",
        EquipmentCategory.SafetyEquipment => "Safety Equipment",
        _ => "Other",
    };

    public static EquipmentCategory ParseEquipmentCategory(string? v) => v?.ToLowerInvariant() switch
    {
        "excavator" => EquipmentCategory.Excavator,
        "tlb" => EquipmentCategory.TLB,
        "grader" => EquipmentCategory.Grader,
        "roller" => EquipmentCategory.Roller,
        "generator" => EquipmentCategory.Generator,
        "compressor" => EquipmentCategory.Compressor,
        "scaffolding" => EquipmentCategory.Scaffolding,
        "power tool" => EquipmentCategory.PowerTool,
        "safety equipment" => EquipmentCategory.SafetyEquipment,
        _ => EquipmentCategory.Other,
    };

    public static bool IsExpiringSoon(DateTime? date, int days = 30) =>
        date.HasValue && date.Value <= DateTime.UtcNow.Date.AddDays(days);
}
