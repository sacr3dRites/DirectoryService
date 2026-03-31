using CSharpFunctionalExtensions;
using DirectoryService.Shared.CustomErrors;

namespace DirectoryService.Domain.Departments.ValueObjects;

public record DepartmentIdentifier
{
    private const int MIN_LENGTH = 3;
    private const int MAX_LENGTH = 150;

    private DepartmentIdentifier(string identifier)
    {
        Value = identifier;
    }

    public string Value { get; }

    public static Result<DepartmentIdentifier, Errors> Create(string identifier)
    {
        var errors = new List<Error>();

        if (identifier.Length > MAX_LENGTH || identifier.Length < MIN_LENGTH)
        {
            errors.Add(GeneralErrors.ValueIsInvalid("длина идентификатора"));
        }

        if (string.IsNullOrWhiteSpace(identifier))
        {
            errors.Add(GeneralErrors.ValueIsInvalid("идентификатор"));
        }

        if (errors.Any())
            return new Errors(errors);

        return new DepartmentIdentifier(identifier);
    }
}