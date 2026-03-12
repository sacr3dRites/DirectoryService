using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Domain.Shared;

public class DepartmentLocation
{
    private DepartmentLocation()
    {
    }

    private DepartmentLocation(Guid locationId, Guid departmentId, Location location, Department department)
    {
        DepartmentLocationId = Guid.NewGuid();
        LocationId = locationId;
        DepartmentId = departmentId;
        Location = location;
        Department = department;
    }

    public Guid DepartmentLocationId { get; }

    public Guid LocationId { get; }

    public Guid DepartmentId { get; }

    public Location Location { get; }

    public Department Department { get; }

    public static Result<DepartmentLocation> Create(Location location, Department department)
    {
        if (location is null)
            return Result.Failure<DepartmentLocation>("Локация не может быть null");
        if (department is null)
            return Result.Failure<DepartmentLocation>("Подразделение не может быть null");

        return Result.Success(new DepartmentLocation(location.Id, department.Id, location, department));
    }
}