using System.Collections;

namespace DirectoryService.Shared.CustomErrors;

public class Errors : IEnumerable<Error>
{
    private readonly List<Error> _errors;

    public Errors()
    {
        _errors = new List<Error>();
    }

    public Errors(IEnumerable<Error> errors)
    {
        _errors = [..errors];
    }

    public static implicit operator Errors(List<Error> errors) => new(errors);

    public static implicit operator Errors(Error error) => new([error]);

    public IEnumerator<Error> GetEnumerator() => _errors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}