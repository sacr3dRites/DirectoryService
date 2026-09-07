using System.ComponentModel.DataAnnotations;
using DirectoryService.Contracts.Shared;

namespace DirectoryService.Contracts.Departments;

public record GetDepartmentsRequest(
    [StringLength(100)] string? Search,
    SortBy SortBy,
    SortDirection SortDirection,
    [Range(1, int.MaxValue)] int Page = 1,
    [Range(1, 100)] int PageSize = 20);
