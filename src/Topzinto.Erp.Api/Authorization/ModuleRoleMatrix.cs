using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Api.Authorization;

/// <summary>
/// Mirrors frontend <c>roleAccess.ts</c> — keep both in sync when changing nav RBAC.
/// </summary>
public static class ModuleRoleMatrix
{
    private static readonly HashSet<string> AdminRoles =
        new(StringComparer.OrdinalIgnoreCase) { SystemRoles.Director, SystemRoles.SuperAdmin };

    private static readonly HashSet<string> FieldRoles =
        new(StringComparer.OrdinalIgnoreCase)
        {
            SystemRoles.Employee, SystemRoles.Driver, SystemRoles.Foreman, SystemRoles.Supervisor,
        };

    private static readonly HashSet<string> FieldModules =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ErpModules.Dashboard, ErpModules.SiteReports, ErpModules.Schedule, ErpModules.Documents,
            ErpModules.Notifications, ErpModules.Timesheets, ErpModules.Attendance, ErpModules.Chat,
            ErpModules.Search,
            ErpModules.Settings,
        };

    private static readonly Dictionary<string, string[]> Restrictions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [ErpModules.Employees] =
            [
                SystemRoles.HR, SystemRoles.OperationsManager, SystemRoles.ProjectManager,
                SystemRoles.Receptionist,
            ],
            [ErpModules.Timesheets] =
            [
                SystemRoles.HR, SystemRoles.OperationsManager, SystemRoles.ProjectManager,
                SystemRoles.Supervisor, SystemRoles.Foreman,
            ],
            [ErpModules.Fleet] =
            [
                SystemRoles.FleetManager, SystemRoles.EquipmentManager, SystemRoles.OperationsManager,
                SystemRoles.ProjectManager,
            ],
            [ErpModules.Equipment] =
            [
                SystemRoles.EquipmentManager, SystemRoles.FleetManager, SystemRoles.OperationsManager,
                SystemRoles.ProjectManager,
            ],
            [ErpModules.Procurement] =
            [
                SystemRoles.Procurement, SystemRoles.Finance, SystemRoles.StoreController,
                SystemRoles.OperationsManager, SystemRoles.ProjectManager,
            ],
            [ErpModules.Suppliers] =
            [
                SystemRoles.Procurement, SystemRoles.Finance, SystemRoles.OperationsManager,
                SystemRoles.ProjectManager,
            ],
            [ErpModules.Stores] =
            [
                SystemRoles.StoreController, SystemRoles.Procurement, SystemRoles.OperationsManager,
                SystemRoles.ProjectManager,
            ],
            [ErpModules.Boq] =
            [
                SystemRoles.QuantitySurveyor, SystemRoles.Estimator, SystemRoles.Finance,
                SystemRoles.ContractManager, SystemRoles.ProjectManager, SystemRoles.OperationsManager,
            ],
            [ErpModules.Reports] =
            [
                SystemRoles.Finance, SystemRoles.QuantitySurveyor, SystemRoles.ContractManager,
                SystemRoles.OperationsManager, SystemRoles.ProjectManager,
            ],
            [ErpModules.Tenders] =
            [
                SystemRoles.ContractManager, SystemRoles.Estimator, SystemRoles.OperationsManager,
                SystemRoles.ProjectManager, SystemRoles.Receptionist,
            ],
            [ErpModules.Contracts] =
            [
                SystemRoles.ContractManager, SystemRoles.QuantitySurveyor, SystemRoles.OperationsManager,
                SystemRoles.ProjectManager,
            ],
            [ErpModules.Clients] =
            [
                SystemRoles.Receptionist, SystemRoles.ContractManager, SystemRoles.OperationsManager,
                SystemRoles.ProjectManager, SystemRoles.Estimator,
            ],
            [ErpModules.Safety] =
            [
                SystemRoles.SafetyOfficer,
            ],
            [ErpModules.Compliance] =
            [
                SystemRoles.SafetyOfficer, SystemRoles.HR, SystemRoles.OperationsManager,
                SystemRoles.ProjectManager,
            ],
            [ErpModules.Attendance] =
            [
                SystemRoles.HR, SystemRoles.OperationsManager, SystemRoles.ProjectManager,
                SystemRoles.Supervisor, SystemRoles.Foreman,
            ],
        };

    public static bool CanAccess(string module, IEnumerable<string> userRoles)
    {
        var roles = userRoles as IList<string> ?? userRoles.ToList();
        if (roles.Count == 0) return false;

        if (roles.Any(r => AdminRoles.Contains(r)))
            return true;

        if (roles.Any(r => FieldRoles.Contains(r)))
            return FieldModules.Contains(module);

        if (!Restrictions.TryGetValue(module, out var allowed))
            return true;

        return roles.Any(r => allowed.Contains(r, StringComparer.OrdinalIgnoreCase));
    }

    public static IReadOnlyDictionary<string, string[]> BuildAccessMatrix() =>
        ErpModules.All.ToDictionary(
            m => m,
            m => SystemRoles.All.Where(r => CanAccess(m, [r])).ToArray(),
            StringComparer.OrdinalIgnoreCase);
}
