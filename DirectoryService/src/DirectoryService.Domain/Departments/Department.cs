using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Shared;
using DirectoryService.Domain.SharedValueObjects;

namespace DirectoryService.Domain.Departments;

public class Department
{
    private List<DepartmentPosition> _positions = [];
    private List<DepartmentLocation> _locations = [];
    private List<Department> _children = [];

    private Department()
    {
        
    }
    
    private Department(
        CorrectName name,
        DepartmentIdentifier identifier,
        DepartmentPath path,
        Department? parent)
    {
        Id = Guid.NewGuid();
        Name = name;
        Depth = 0;
        IsActive = true;
        Parent = parent;
        Identifier = identifier;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        Path = path;
    }

    public Guid Id { get; }

    public CorrectName Name { get; private set; }

    public DepartmentIdentifier Identifier { get; private set; }

    public Department? Parent { get; private set; }

    public DepartmentPath Path { get; private set; }

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<Department> Children => _children;

    public IReadOnlyList<DepartmentLocation> Locations => _locations;

    public IReadOnlyList<DepartmentPosition> Positions => _positions;

    public static Department Create(
        DepartmentIdentifier identifier,
        CorrectName name,
        Department? parent)
    {
        var path = DepartmentPath.Create(identifier, parent);

        return new Department(name, identifier, path, parent);
    }

    public Result AddDepartmentPositions(IEnumerable<DepartmentPosition> positions)
    {
        if (!IsActive)
            return Result.Failure("Нельзя добавлять в неактивное подразделение");

        var departmentPositions = positions.ToList();
        var duplicates = departmentPositions
            .Where(p => _positions.Any(x => x.DepartmentPositionId == p.DepartmentPositionId))
            .ToList();

        if (duplicates.Any())
            return Result.Failure("Некоторые позиции уже добавлены");

        UpdatedAt = DateTime.UtcNow;
        _positions.AddRange(departmentPositions);
        return Result.Success();
    }

    public Result AddDepartmentLocations(IEnumerable<DepartmentLocation> locations)
    {
        if (!IsActive)
            return Result.Failure("Нельзя добавлять в неактивное подразделение");

        var departmentLocations = locations.ToList();
        var duplicates = departmentLocations
            .Where(l => _locations.Any(x => x.DepartmentLocationId == l.DepartmentLocationId))
            .ToList();

        if (duplicates.Any())
            return Result.Failure("Некоторые локации уже добавлены");

        _locations.AddRange(departmentLocations);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result AddChildren(IEnumerable<Department> departments)
    {
        if (!IsActive)
            return Result.Failure("Нельзя добавлять в неактивное подразделение");

        foreach (var department in departments)
        {
            if (_children.Any(c => c.Id == department.Id))
                return Result.Failure($"Подразделение {department.Id} уже добавлено");

            if (this.IsAncestorOf(department))
                return Result.Failure("Нельзя создавать цикл в иерархии подразделений");

            department.Parent = this;
            department.Depth = (short)(Depth + 1);
            department.Path = DepartmentPath.Create(department.Identifier, this);
            department.UpdatedAt = DateTime.UtcNow;

            _children.Add(department);
        }

        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    private bool IsAncestorOf(Department department)
    {
        var current = department.Parent;

        while (current != null)
        {
            if (current.Id == Id)
                return true;

            current = current.Parent;
        }

        return false;
    }

    public Result RemoveChildren(IEnumerable<Department> departments)
    {
        if (!IsActive)
            return Result.Failure("Нельзя удалять под-подразделения в неактивном департаменте");

        var ids = departments.Select(p => p.Id).ToHashSet();

        if (ids.Any(id => _children.All(p => p.Id != id)))
            return Result.Failure("Некоторые под-подразделения не найдены");

        var removed = _children.Where(p => ids.Contains(p.Id)).ToList();

        foreach (var child in removed)
        {
            child.Parent = null;
            child.Depth = 0;
        }

        _children.RemoveAll(p => ids.Contains(p.Id));
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RemoveDepartmentPositions(IEnumerable<DepartmentPosition> positions)
    {
        if (!IsActive)
            return Result.Failure("Нельзя удалять позиции в неактивном департаменте");

        var ids = positions.Select(p => p.DepartmentPositionId).ToHashSet();

        if (ids.Any(id => _positions.All(p => p.DepartmentPositionId != id)))
            return Result.Failure("Некоторые позиции не найдены");

        _positions.RemoveAll(p => ids.Contains(p.DepartmentPositionId));
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RemoveDepartmentLocations(IEnumerable<DepartmentLocation> locations)
    {
        if (!IsActive)
            return Result.Failure("Нельзя удалять локации в неактивном департаменте");

        var ids = locations.Select(p => p.DepartmentLocationId).ToHashSet();

        if (ids.Any(id => _locations.All(p => p.DepartmentLocationId != id)))
            return Result.Failure("Некоторые локации не найдены");

        _locations.RemoveAll(p => ids.Contains(p.DepartmentLocationId));
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public void ChangeActiveStatus(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
}