using System.ComponentModel.DataAnnotations;
using DirectoryService.Contracts.Shared;

namespace DirectoryService.Application.Departments.GetAllDepartmentChildren;

public record GetAllDepartmentChildrenQuery(
    Guid DepartmentId,
    SortBy SortBy,
    SortDirection SortDirection,
    [Range(1, Int32.MaxValue)] int Page = 1,
    [Range(1, 100)] int PageSize = 20);