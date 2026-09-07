using System.ComponentModel.DataAnnotations;
using DirectoryService.Contracts.Shared;

namespace DirectoryService.Application.Locations.GetLocations;

public record GetLocationsQuery(
    [StringLength(100)] string? Search,
    int minDepartmentCount,
    SortBy SortBy,
    SortDirection SortDirection,
    [Range(1, Int32.MaxValue)] int Page = 1,
    [Range(1, 100)] int PageSize = 20
);