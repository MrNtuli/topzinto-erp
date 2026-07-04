namespace Topzinto.Erp.Application.DTOs.Search;

public record SearchResultDto(
    string Type,
    Guid Id,
    string Title,
    string Subtitle,
    string LinkPath
);
