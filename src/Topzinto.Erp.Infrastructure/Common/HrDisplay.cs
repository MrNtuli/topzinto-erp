using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Infrastructure.Common;

public static class HrDisplay
{
    public static string FormatDepartment(EmployeeDepartment d) => d switch
    {
        EmployeeDepartment.Site => "Site",
        EmployeeDepartment.Administration => "Administration",
        EmployeeDepartment.Fleet => "Fleet",
        EmployeeDepartment.Procurement => "Procurement",
        EmployeeDepartment.Finance => "Finance",
        EmployeeDepartment.Management => "Management",
        EmployeeDepartment.Safety => "Safety",
        _ => d.ToString(),
    };

    public static EmployeeDepartment ParseDepartment(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "site" => EmployeeDepartment.Site,
            "administration" or "admin" => EmployeeDepartment.Administration,
            "fleet" => EmployeeDepartment.Fleet,
            "procurement" => EmployeeDepartment.Procurement,
            "finance" => EmployeeDepartment.Finance,
            "management" => EmployeeDepartment.Management,
            "safety" => EmployeeDepartment.Safety,
            _ => EmployeeDepartment.Site,
        };

    public static string FormatTrade(EmployeeTrade t) => t switch
    {
        EmployeeTrade.General => "General",
        EmployeeTrade.Bricklayer => "Bricklayer",
        EmployeeTrade.Carpenter => "Carpenter",
        EmployeeTrade.Electrician => "Electrician",
        EmployeeTrade.Plumber => "Plumber",
        EmployeeTrade.SteelFixer => "Steel Fixer",
        EmployeeTrade.Operator => "Operator",
        EmployeeTrade.Driver => "Driver",
        EmployeeTrade.Supervisor => "Supervisor",
        EmployeeTrade.Foreman => "Foreman",
        EmployeeTrade.Other => "Other",
        _ => t.ToString(),
    };

    public static EmployeeTrade ParseTrade(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "general" => EmployeeTrade.General,
            "bricklayer" => EmployeeTrade.Bricklayer,
            "carpenter" => EmployeeTrade.Carpenter,
            "electrician" => EmployeeTrade.Electrician,
            "plumber" => EmployeeTrade.Plumber,
            "steel fixer" or "steelfixer" => EmployeeTrade.SteelFixer,
            "operator" => EmployeeTrade.Operator,
            "driver" => EmployeeTrade.Driver,
            "supervisor" => EmployeeTrade.Supervisor,
            "foreman" => EmployeeTrade.Foreman,
            "other" => EmployeeTrade.Other,
            _ => EmployeeTrade.General,
        };

    public static string FormatStatus(EmploymentStatus s) => s switch
    {
        EmploymentStatus.Active => "Active",
        EmploymentStatus.OnLeave => "On Leave",
        EmploymentStatus.Suspended => "Suspended",
        EmploymentStatus.Terminated => "Terminated",
        _ => s.ToString(),
    };

    public static EmploymentStatus ParseStatus(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "active" => EmploymentStatus.Active,
            "on leave" or "onleave" => EmploymentStatus.OnLeave,
            "suspended" => EmploymentStatus.Suspended,
            "terminated" => EmploymentStatus.Terminated,
            _ => EmploymentStatus.Active,
        };

    public static string FormatDate(DateTime? d) => d?.ToString("yyyy-MM-dd") ?? "";

    public static DateTime? ParseDate(string? value) =>
        DateTime.TryParse(value, out var d) ? d.Date : null;

    public static string FormatTimesheetStatus(TimesheetStatus s) => s switch
    {
        TimesheetStatus.Draft => "Draft",
        TimesheetStatus.Submitted => "Submitted",
        TimesheetStatus.Approved => "Approved",
        _ => s.ToString(),
    };

    public static TimesheetStatus ParseTimesheetStatus(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "draft" => TimesheetStatus.Draft,
            "submitted" => TimesheetStatus.Submitted,
            "approved" => TimesheetStatus.Approved,
            _ => TimesheetStatus.Draft,
        };
}
