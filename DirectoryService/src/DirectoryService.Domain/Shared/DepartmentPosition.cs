using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Shared;

public class DepartmentPosition
{
    private DepartmentPosition()
    {
    }

    private DepartmentPosition(Guid positionId, Guid departmentId, Position position, Department department)
    {
        DepartmentPositionId = Guid.NewGuid();
        PositionId = positionId;
        DepartmentId = departmentId;
        Position = position;
        Department = department;
    }

    public Guid DepartmentPositionId { get; }

    public Guid PositionId { get; }

    public Guid DepartmentId { get; }

    public Position Position { get; }

    public Department Department { get; }

    public static Result<DepartmentPosition, Errors> Create(Position position, Department department)
    {
        var errors = new List<Error>();

        if (position is null)
            errors.Add(GeneralErrors.ValueIsRequired("позиция"));
        if (department is null)
            errors.Add(GeneralErrors.ValueIsRequired("подразделение"));

        if (errors.Any())
            return new Errors(errors);

        return new DepartmentPosition(position.Id, department.Id, position, department);
    }
}