namespace Topzinto.Erp.Domain.Enums;

public enum VehicleStatus
{
    Available = 0,
    InUse = 1,
    Maintenance = 2,
    Decommissioned = 3,
}

public enum VehicleType
{
    LDV = 0,
    Bakkie = 1,
    Truck = 2,
    TipperTruck = 3,
    TLB = 4,
    WaterTanker = 5,
    HeavyVehicle = 6,
    Other = 7,
}

public enum EquipmentStatus
{
    Available = 0,
    InUse = 1,
    Maintenance = 2,
    Decommissioned = 3,
}

public enum EquipmentCategory
{
    Excavator = 0,
    TLB = 1,
    Grader = 2,
    Roller = 3,
    Generator = 4,
    Compressor = 5,
    Scaffolding = 6,
    PowerTool = 7,
    SafetyEquipment = 8,
    Other = 9,
}
