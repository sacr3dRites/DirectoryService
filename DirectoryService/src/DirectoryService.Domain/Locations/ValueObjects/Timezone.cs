using CSharpFunctionalExtensions;
using DirectoryService.Shared.CustomErrors;

namespace DirectoryService.Domain.Locations.ValueObjects;

public record Timezone
{
    private Timezone(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public static Result<Timezone, Errors> Create(string name)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(GeneralErrors.ValueIsRequired("временная зона"));

        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(name);
        }
        catch (Exception)
        {
            errors.Add(GeneralErrors.ValueIsInvalid("IANA код"));
        }

        return new Timezone(name);
    }
}