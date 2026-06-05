using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Infrastructure.Database;

public interface IReadDbContext
{
    IQueryable<Department> DepartmentsRead { get; }

    IQueryable<Location> LocationsRead { get; }
}