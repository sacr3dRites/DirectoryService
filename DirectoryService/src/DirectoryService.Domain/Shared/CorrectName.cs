using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.SharedValueObjects;

public record CorrectName
{
    private string _value;
    private const int MIN_LENGTH = 3;

    private CorrectName(string value)
    {
        Value = value;
    }

    public string Value
    {
        get => _value;
        set => _value = value;
    }

    public static Result<CorrectName> Create(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<CorrectName>("Некорректное имя");
        }

        if (value.Length < MIN_LENGTH || value.Length > maxLength)
        {
            return Result.Failure<CorrectName>("Некорректная длина имени");
        }

        return Result.Success(new CorrectName(value));
    }
}