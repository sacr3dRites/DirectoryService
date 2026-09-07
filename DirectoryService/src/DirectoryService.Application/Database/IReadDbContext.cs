using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Database;

public interface IReadDbContext
{
    IQueryable<Department> DepartmentsRead { get; }

    IQueryable<Location> LocationsRead { get; }
    IQueryable<DepartmentLocation> DepartmentLocationsRead { get; }
}