namespace Topzinto.Erp.Domain.Enums;

public enum EmployeeDepartment
{
    Site,
    Administration,
    Fleet,
    Procurement,
    Finance,
    Management,
    Safety
}

public enum EmployeeTrade
{
    General,
    Bricklayer,
    Carpenter,
    Electrician,
    Plumber,
    SteelFixer,
    Operator,
    Driver,
    Supervisor,
    Foreman,
    Other
}

public enum EmploymentStatus
{
    Active,
    OnLeave,
    Suspended,
    Terminated
}

public enum TimesheetStatus
{
    Draft,
    Submitted,
    Approved
}

public enum AttendanceStatus
{
    Present,
    Absent,
    Late,
    HalfDay,
    OnLeave,
    PublicHoliday
}
