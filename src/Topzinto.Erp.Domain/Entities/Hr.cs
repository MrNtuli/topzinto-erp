using Topzinto.Erp.Domain.Common;
using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Domain.Entities;

public class Employee : BaseEntity
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? IdNumber { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public EmployeeDepartment Department { get; set; } = EmployeeDepartment.Site;
    public EmployeeTrade Trade { get; set; } = EmployeeTrade.General;
    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime HireDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime? TerminationDate { get; set; }
    public Guid? AssignedProjectId { get; set; }
    public Project? AssignedProject { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? Notes { get; set; }
}

public class TimesheetEntry : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public DateTime WorkDate { get; set; } = DateTime.UtcNow.Date;
    public decimal Hours { get; set; }
    public TimesheetStatus Status { get; set; } = TimesheetStatus.Draft;
    public string? Description { get; set; }
    public string? Notes { get; set; }
}

public class AttendanceRecord : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }
    public DateTime WorkDate { get; set; } = DateTime.UtcNow.Date;
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public decimal? HoursWorked { get; set; }
    public string? Notes { get; set; }
}
