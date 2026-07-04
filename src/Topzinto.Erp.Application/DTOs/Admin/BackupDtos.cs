namespace Topzinto.Erp.Application.DTOs.Admin;

public record BackupFileDto(string FileName, long SizeBytes, string CreatedAt);

public record BackupHubDto(
    string Engine,
    bool ScheduleEnabled,
    int IntervalHours,
    int RetentionCount,
    IReadOnlyList<BackupFileDto> Files
);

public record CreateBackupResultDto(string FileName, string Message);
