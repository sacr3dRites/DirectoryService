using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Domain.Shared;
using DirectoryService.Domain.SharedValueObjects;

namespace DirectoryService.Domain.Locations;

public class Location
{
    private List<DepartmentLocation> _departments = [];

    private Location(CorrectName name, LocationAddress locationAddress, Timezone timezone)
    {
        Id = Guid.NewGuid();
        Name = name;
        LocationAddress = locationAddress;
        Timezone = timezone;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public CorrectName Name { get; private set; }

    public LocationAddress LocationAddress { get; private set; }

    public Timezone Timezone { get; private set; }

    private bool IsActive { get; set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentLocation> Departments => _departments;

    public Result AddDepartment(IEnumerable<DepartmentLocation> departments)
    {
        if (!IsActive)
            return Result.Failure("Нельзя добавлять подразделения в неактивную локацию");

        var departmentLocations = departments.ToList();
        var duplicates = departmentLocations
            .Where(l => _departments.Any(x => x.DepartmentLocationId == l.DepartmentLocationId))
            .ToList();

        if (duplicates.Any())
            return Result.Failure("Некоторые локации уже добавлены");

        _departments.AddRange(departmentLocations);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RemoveDepartment(IEnumerable<DepartmentLocation> departments)
    {
        if (!IsActive)
            return Result.Failure("Нельзя удалять подразделения в неактивной локации");

        var ids = departments.Select(p => p.DepartmentLocationId).ToHashSet();

        if (ids.Any(id => _departments.All(p => p.DepartmentLocationId != id)))
            return Result.Failure("Некоторые департаменты не найдены");

        _departments.RemoveAll(p => ids.Contains(p.DepartmentLocationId));

        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public void ChangeActiveStatus(bool isActive)
    {
        IsActive = isActive;
        this.UpdatedAt = DateTime.UtcNow;
    }

    public static Result<Location> Create(string name, string address, string timezone)
    {
        var correctNameResult = CorrectName.Create(name, 120);

        if (correctNameResult.IsFailure)
        {
            return Result.Failure<Location>(correctNameResult.Error);
        }

        var addressResult = LocationAddress.Create(address);

        if (addressResult.IsFailure)
        {
            return Result.Failure<Location>(addressResult.Error);
        }

        var timezoneResult = Timezone.Create(timezone);

        if (timezoneResult.IsFailure)
        {
            return Result.Failure<Location>(timezoneResult.Error);
        }

        return new Location(correctNameResult.Value, addressResult.Value, timezoneResult.Value);
    }
}