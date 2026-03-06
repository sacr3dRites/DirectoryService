using CSharpFunctionalExtensions;

namespace DirectoryService.Domain;

public record DepartmentIdentifier
{
    private const int MIN_LENGTH = 3;
    private const int MAX_LENGTH = 150;

    private DepartmentIdentifier(string identifier)
    {
        Value = identifier;
    }

    public string Value { get; }

    public static Result<DepartmentIdentifier> Create(string identifier)
    {
        if (identifier.Length > MAX_LENGTH || identifier.Length < MIN_LENGTH)
        {
            return Result.Failure<DepartmentIdentifier>("Некорректная длина идентификатора");
        }

        if (string.IsNullOrWhiteSpace(identifier))
        {
            return Result.Failure<DepartmentIdentifier>("Некорректный идентификатор");
        }

        return Result.Success(new DepartmentIdentifier(identifier));
    }
}