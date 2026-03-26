using System.Text.Json.Serialization;

namespace DirectoryService.Shared.EndpointResults;

public record Envelope
{
    public object? Result;

    public Errors Errors;

    public DateTime TimeGenerated;

    [JsonConstructor]
    private Envelope(object? result, Errors errors)
    {
        Errors = errors;
        Result = result;
        TimeGenerated = DateTime.UtcNow;
    }

    public static Envelope Ok(object? result = default) =>
        new(result, null);

    public static Envelope Error(Errors errors) =>
        new(default, errors);
}

public record Envelope<T>
{
    public T? Result;

    public Errors Errors;

    public DateTime TimeGenerated;

    [JsonConstructor]
    private Envelope(T? result, Errors errors)
    {
        Errors = errors;
        Result = result;
        TimeGenerated = DateTime.UtcNow;
    }

    public static Envelope<T> Ok(T? result = default) =>
        new(result, null);

    public static Envelope<T> Error(Errors errors) =>
        new(default, errors);
}