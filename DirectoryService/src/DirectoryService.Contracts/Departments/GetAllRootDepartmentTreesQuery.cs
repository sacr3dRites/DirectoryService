using System.ComponentModel.DataAnnotations;
using DirectoryService.Contracts.Shared;

namespace DirectoryService.Contracts.Departments;

public record GetAllRootDepartmentTreesQuery(
    SortBy SortBy,
    SortDirection SortDirection,
    [Range(1, Int32.MaxValue)] int Page = 1,
    [Range(1, 100)] int PageSize = 20);