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

    public static Result<CorrectPositionName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<CorrectPositionName>("Некорректное имя позиции");
        }

        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return Result.Failure<CorrectPositionName>("Некорректная длина имени позиции");
        }

        return Result.Success(new CorrectPositionName(value));
    }
}