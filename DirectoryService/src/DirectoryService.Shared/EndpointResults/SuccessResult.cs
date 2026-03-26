using System.Net;
using Microsoft.AspNetCore.Http;

namespace DirectoryService.Shared.EndpointResults;

public class SuccessResult<T> : IResult
{
    private readonly T _value;

    public SuccessResult(T value)
    {
        _value = value;
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var envelope = Envelope.Ok(_value);

        httpContext.Response.StatusCode = (int)HttpStatusCode.OK;

        return httpContext.Response.WriteAsJsonAsync(envelope);
    }
}