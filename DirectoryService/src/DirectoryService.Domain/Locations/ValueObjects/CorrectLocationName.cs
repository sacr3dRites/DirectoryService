using CSharpFunctionalExtensions;

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

    public static Result<CorrectLocationName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<CorrectLocationName>("Некорректное имя локации");
        }

        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return Result.Failure<CorrectLocationName>("Некорректная длина имени локации");
        }

        return Result.Success(new CorrectLocationName(value));
    }
}