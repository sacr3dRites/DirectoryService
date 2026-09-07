using System.ComponentModel.DataAnnotations;

namespace DirectoryService.Application.Departments.GetSelectedDepartmentTree;

public record GetSelectedDepartmentTreeQuery(
    [MinLength(2)] string Search,
    [Range(1, Int32.MaxValue)] int Page = 1,
    [Range(1, 100)] int PageSize = 20);