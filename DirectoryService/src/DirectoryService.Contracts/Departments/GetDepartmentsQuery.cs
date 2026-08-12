using System.ComponentModel.DataAnnotations;

namespace DirectoryService.Contracts.Departments;

public record GetDepartmentsQuery(
    [StringLength(100)] string? Search,
    SortBy SortBy,
    SortDirection SortDirection,
    [Range(1, Int32.MaxValue)] int Page = 1,
    [Range(1, 100)] int PageSize = 20);