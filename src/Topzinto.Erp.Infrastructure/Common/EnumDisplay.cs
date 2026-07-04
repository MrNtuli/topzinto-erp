using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Infrastructure.Common;

public static class EnumDisplay
{
    public static string FormatProjectStatus(ProjectStatus s) => s switch
    {
        ProjectStatus.Planned => "Planned",
        ProjectStatus.Active => "In Progress",
        ProjectStatus.OnHold => "On Hold",
        ProjectStatus.Completed => "Completed",
        _ => s.ToString(),
    };

    public static ProjectStatus ParseProjectStatus(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "planned" => ProjectStatus.Planned,
            "active" or "in progress" => ProjectStatus.Active,
            "on hold" or "onhold" => ProjectStatus.OnHold,
            "completed" => ProjectStatus.Completed,
            _ => ProjectStatus.Planned,
        };

    public static string FormatTenderStatus(TenderStatus s) => s switch
    {
        TenderStatus.Identified => "Identified",
        TenderStatus.Preparing => "Preparing",
        TenderStatus.Submitted => "Submitted",
        TenderStatus.Won => "Won",
        TenderStatus.Lost => "Lost",
        _ => s.ToString(),
    };

    public static TenderStatus ParseTenderStatus(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "identified" => TenderStatus.Identified,
            "preparing" => TenderStatus.Preparing,
            "submitted" => TenderStatus.Submitted,
            "won" => TenderStatus.Won,
            "lost" => TenderStatus.Lost,
            _ => TenderStatus.Identified,
        };

    public static string FormatContractStatus(ContractStatus s) => s switch
    {
        ContractStatus.Draft => "Draft",
        ContractStatus.Active => "Active",
        ContractStatus.Completed => "Completed",
        ContractStatus.Terminated => "Terminated",
        _ => s.ToString(),
    };

    public static ContractStatus ParseContractStatus(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "draft" => ContractStatus.Draft,
            "active" => ContractStatus.Active,
            "completed" => ContractStatus.Completed,
            "terminated" => ContractStatus.Terminated,
            _ => ContractStatus.Draft,
        };

    public static string FormatClientType(ClientType t) => t switch
    {
        ClientType.Government => "Government",
        ClientType.Municipal => "Municipal",
        ClientType.Private => "Private",
        ClientType.SOE => "SOE",
        _ => t.ToString(),
    };

    public static ClientType ParseClientType(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "government" => ClientType.Government,
            "municipal" => ClientType.Municipal,
            "private" => ClientType.Private,
            "soe" => ClientType.SOE,
            _ => ClientType.Private,
        };
}
