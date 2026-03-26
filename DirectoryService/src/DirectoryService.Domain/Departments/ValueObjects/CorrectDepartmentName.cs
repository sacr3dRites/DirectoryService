using CSharpFunctionalExtensions;
using DirectoryService.Shared;

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

    public static Result<CorrectDepartmentName, Errors> Create(string value)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(value))
            errors.Add(GeneralErrors.ValueIsInvalid("имя департамента"));

        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
            errors.Add(GeneralErrors.ValueIsInvalid("длина имени департамента"));

        if (errors.Any())
            return new Errors(errors);


        return new CorrectDepartmentName(value);
    }
}