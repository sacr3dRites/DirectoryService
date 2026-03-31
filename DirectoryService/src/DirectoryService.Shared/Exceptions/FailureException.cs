using DirectoryService.Shared.CustomErrors;

namespace DirectoryService.Shared.Exceptions;

public class FailureException : Exception
{
    public FailureException(Error error, string? message = null)
        : base(message)
    {
        Error = error;
    }

    public Error Error { get; set; }
}