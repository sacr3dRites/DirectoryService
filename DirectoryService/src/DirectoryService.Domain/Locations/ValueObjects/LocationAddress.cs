using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.CustomErrors;

namespace DirectoryService.Domain.Locations.ValueObjects;

public record LocationAddress
{
    private LocationAddress(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<LocationAddress, Errors> Create(string address)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(address))
            errors.Add(GeneralErrors.ValueIsRequired("адрес"));

        var regex = new Regex(@"^[\w\s.,\-/']+$");
        if (!regex.IsMatch(address))
        {
            errors.Add(
                GeneralErrors.ValueIsInvalid($"Использованы недопустимые символы в адресе - {address}, значение"));
        }

        if (errors.Any())
            return new Errors(errors);

        return new LocationAddress(address);
    }
}