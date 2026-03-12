using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Locations.ValueObjects;

public record Timezone
{
    private Timezone(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public static Result<Timezone> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Timezone>("Временная зона не может быть пустой");

        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(name);
        }
        catch (Exception)
        {
            return Result.Failure<Timezone>($"Некорректный IANA код");
        }

        return Result.Success(new Timezone(name));
    }
}