namespace Topzinto.Erp.Api.Authorization;

public static class ErpModules
{
    public const string Dashboard = "Dashboard";
    public const string Projects = "Projects";
    public const string SiteReports = "SiteReports";
    public const string Schedule = "Schedule";
    public const string Documents = "Documents";
    public const string Notifications = "Notifications";
    public const string Chat = "Chat";
    public const string Timesheets = "Timesheets";
    public const string Employees = "Employees";
    public const string Fleet = "Fleet";
    public const string Equipment = "Equipment";
    public const string Procurement = "Procurement";
    public const string Suppliers = "Suppliers";
    public const string Stores = "Stores";
    public const string Boq = "Boq";
    public const string Reports = "Reports";
    public const string Tenders = "Tenders";
    public const string Contracts = "Contracts";
    public const string Clients = "Clients";
    public const string Search = "Search";
    public const string Settings = "Settings";

    public const string PolicyPrefix = "Module:";

    public static string Policy(string module) => PolicyPrefix + module;

    public const string DashboardPolicy = PolicyPrefix + Dashboard;
    public const string ProjectsPolicy = PolicyPrefix + Projects;
    public const string SiteReportsPolicy = PolicyPrefix + SiteReports;
    public const string SchedulePolicy = PolicyPrefix + Schedule;
    public const string DocumentsPolicy = PolicyPrefix + Documents;
    public const string NotificationsPolicy = PolicyPrefix + Notifications;
    public const string ChatPolicy = PolicyPrefix + Chat;
    public const string TimesheetsPolicy = PolicyPrefix + Timesheets;
    public const string EmployeesPolicy = PolicyPrefix + Employees;
    public const string FleetPolicy = PolicyPrefix + Fleet;
    public const string EquipmentPolicy = PolicyPrefix + Equipment;
    public const string ProcurementPolicy = PolicyPrefix + Procurement;
    public const string SuppliersPolicy = PolicyPrefix + Suppliers;
    public const string StoresPolicy = PolicyPrefix + Stores;
    public const string BoqPolicy = PolicyPrefix + Boq;
    public const string ReportsPolicy = PolicyPrefix + Reports;
    public const string TendersPolicy = PolicyPrefix + Tenders;
    public const string ContractsPolicy = PolicyPrefix + Contracts;
    public const string ClientsPolicy = PolicyPrefix + Clients;
    public const string SearchPolicy = PolicyPrefix + Search;
    public const string SettingsPolicy = PolicyPrefix + Settings;

    public static readonly string[] All =
    [
        Dashboard, Projects, SiteReports, Schedule, Documents, Notifications, Chat,
        Timesheets, Employees, Fleet, Equipment, Procurement, Suppliers, Stores,
        Boq, Reports, Tenders, Contracts, Clients, Search, Settings,
    ];
}
