namespace Topzinto.Erp.Domain.Enums;

public static class SystemRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Director = "Director";
    public const string OperationsManager = "OperationsManager";
    public const string ProjectManager = "ProjectManager";
    public const string ContractManager = "ContractManager";
    public const string QuantitySurveyor = "QuantitySurveyor";
    public const string Estimator = "Estimator";
    public const string Receptionist = "Receptionist";
    public const string HR = "HR";
    public const string Finance = "Finance";
    public const string Procurement = "Procurement";
    public const string FleetManager = "FleetManager";
    public const string EquipmentManager = "EquipmentManager";
    public const string SafetyOfficer = "SafetyOfficer";
    public const string StoreController = "StoreController";
    public const string Supervisor = "Supervisor";
    public const string Foreman = "Foreman";
    public const string Driver = "Driver";
    public const string Employee = "Employee";

    public static readonly string[] All =
    [
        SuperAdmin, Director, OperationsManager, ProjectManager, ContractManager,
        QuantitySurveyor, Estimator, Receptionist, HR, Finance, Procurement,
        FleetManager, EquipmentManager, SafetyOfficer, StoreController,
        Supervisor, Foreman, Driver, Employee
    ];
}
