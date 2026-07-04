namespace Topzinto.Erp.Application.DTOs.Equipment;

public record EquipmentSummaryDto(int Total, int Available, int InUse, int Maintenance, int InspectionDue);

public record EquipmentDto(
    Guid Id,
    string AssetTag,
    string Name,
    string Category,
    string Status,
    string? OperatorName,
    string? AssignedProjectName,
    string? NextServiceDue,
    bool IsInspectionDue
);

public record EquipmentDetailDto(
    Guid Id,
    string AssetTag,
    string Name,
    string Category,
    string Status,
    string? MakeModel,
    string? SerialNumber,
    string? OperatorName,
    string? LastInspectionDate,
    string? NextInspectionDue,
    string? LastServiceDate,
    string? NextServiceDue,
    Guid? AssignedProjectId,
    string? AssignedProjectName,
    string? Notes,
    IReadOnlyList<BookingDto> Bookings,
    IReadOnlyList<InspectionDto> Inspections,
    string? NextInspectionDueInput,
    string? NextServiceDueInput
);

public record BookingDto(Guid Id, string ProjectName, string StartDate, string EndDate, string? BookedByName);
public record InspectionDto(Guid Id, string InspectionDate, string Result, string? InspectorName, string? NextDueDate);

public record CreateEquipmentRequest(
    string AssetTag,
    string Name,
    string Category,
    string Status,
    string? MakeModel,
    string? SerialNumber,
    string? OperatorName,
    DateTime? NextInspectionDue,
    DateTime? NextServiceDue,
    Guid? AssignedProjectId,
    string? Notes
);

public record UpdateEquipmentRequest(
    string AssetTag,
    string Name,
    string Category,
    string Status,
    string? MakeModel,
    string? SerialNumber,
    string? OperatorName,
    DateTime? NextInspectionDue,
    DateTime? NextServiceDue,
    Guid? AssignedProjectId,
    string? Notes
);
