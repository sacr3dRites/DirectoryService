using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain;

public record LocationAddress
{
    private LocationAddress() { }

    private LocationAddress(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<LocationAddress> Create(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return Result.Failure<LocationAddress>("Адрес не может быть пустым");

        var regex = new Regex(@"^[\w\s.,\-/']+$");
        if (!regex.IsMatch(address))
            return Result.Failure<LocationAddress>($"Использованы недопустимые символы в адресе - {address}");

        return Result.Success(new LocationAddress(address));
    }
}