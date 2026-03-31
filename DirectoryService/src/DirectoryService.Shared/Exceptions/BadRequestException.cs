using DirectoryService.Shared.CustomErrors;

namespace DirectoryService.Shared.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException(Error error, string? message = null)
        : base(message)
    {
        Error = error;
    }

    public Error Error { get; }
}