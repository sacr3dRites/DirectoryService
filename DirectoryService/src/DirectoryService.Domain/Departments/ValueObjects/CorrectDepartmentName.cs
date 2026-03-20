using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments.ValueObjects;

public record CorrectDepartmentName
{
    private string _value;
    private const int MIN_LENGTH = 3;
    private const int MAX_LENGTH = 150;

    private CorrectDepartmentName(string value)
    {
        Value = value;
    }

    public string Value
    {
        get => _value;
        set => _value = value;
    }

    public static Result<CorrectDepartmentName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<CorrectDepartmentName>("Некорректное имя департамента");
        }

        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return Result.Failure<CorrectDepartmentName>("Некорректная длина имени департамента");
        }

        return Result.Success(new CorrectDepartmentName(value));
    }
}