using DirectoryService.Shared.CustomErrors;

namespace DirectoryService.Shared.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(Error error, string? message = null)
        : base(message)
    {
        Error = error;
    }

    public Error Error { get; set; }
}