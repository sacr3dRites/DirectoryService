using DirectoryService.Shared;

namespace DirectoryService.Domain.Positions;

using CSharpFunctionalExtensions;

public record CorrectPositionName
{
    private string _value;
    private const int MIN_LENGTH = 3;
    private const int MAX_LENGTH = 100;

    private CorrectPositionName(string value)
    {
        Value = value;
    }

    public string Value
    {
        get => _value;
        set => _value = value;
    }

    public static Result<CorrectPositionName, Errors> Create(string value)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(value))
            errors.Add(GeneralErrors.ValueIsInvalid("имя позиции"));

        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
            errors.Add(GeneralErrors.ValueIsInvalid("длина имени позиции"));

        if (errors.Any())
            return new Errors(errors);

        return new CorrectPositionName(value);
    }
}