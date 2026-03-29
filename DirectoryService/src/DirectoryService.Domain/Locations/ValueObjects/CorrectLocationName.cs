using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Locations.ValueObjects;

public record CorrectLocationName
{
    private string _value;
    private const int MIN_LENGTH = 3;
    private const int MAX_LENGTH = 120;

    private CorrectLocationName(string value)
    {
        Value = value;
    }

    public string Value
    {
        get => _value;
        set => _value = value;
    }

    public static Result<CorrectLocationName, Errors> Create(string value)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(value))
            errors.Add(GeneralErrors.ValueIsInvalid("имя локации"));

        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
            errors.Add(GeneralErrors.ValueIsInvalid("длина имени локации"));

        if (errors.Any())
            return new Errors(errors);

        return new CorrectLocationName(value);
    }
}