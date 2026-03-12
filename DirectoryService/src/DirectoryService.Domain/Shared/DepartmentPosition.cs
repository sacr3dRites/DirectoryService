using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;

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

    public static Result<DepartmentPosition> Create(Position position, Department department)
    {
        if (position is null)
            return Result.Failure<DepartmentPosition>("Позиция не может быть null");
        if (department is null)
            return Result.Failure<DepartmentPosition>("Подразделение не может быть null");

        return Result.Success(new DepartmentPosition(position.Id, department.Id, position, department));
    }
}