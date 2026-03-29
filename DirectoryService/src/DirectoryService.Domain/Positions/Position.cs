using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Positions;

public class Position
{
    private const int MAX_DESCRIPTION_LENGTH = 1000;

    private Position()
    {
    }

    private Position(CorrectPositionName name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public CorrectPositionName Name { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public Result ChangeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure("Описание не может быть пустым");

        if (description.Length > MAX_DESCRIPTION_LENGTH)
            return Result.Failure("Размер описания должен быть не больше либо равен 1000 знакам");

        Description = description;
        this.UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public void ChangeActiveStatus(bool isActive)
    {
        IsActive = isActive;
        this.UpdatedAt = DateTime.UtcNow;
    }

    public static Result<Position, Errors> Create(CorrectPositionName name, string description)
    {
        if (description.Length > MAX_DESCRIPTION_LENGTH)
        {
            return Error.Validation("value.is.invalid", "Размер описания должен быть не больше либо равен 1000 знакам")
                .ToErrors();
        }

        return new Position(name, description);
    }
}