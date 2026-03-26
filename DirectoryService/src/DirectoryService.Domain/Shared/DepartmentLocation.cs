using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Shared;

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

    public static Result<DepartmentLocation, Errors> Create(Location location, Department department)
    {
        var errors = new List<Error>();

        if (location is null)
            errors.Add(GeneralErrors.ValueIsRequired("локация"));
        if (department is null)
            errors.Add(GeneralErrors.ValueIsRequired("подразделение"));

        if (errors.Any())
            return new Errors(errors);

        return new DepartmentLocation(location.Id, department.Id, location, department);
    }
}