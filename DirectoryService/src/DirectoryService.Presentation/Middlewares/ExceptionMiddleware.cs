using DirectoryService.Shared.CustomErrors;
using DirectoryService.Shared.EndpointResults;
using DirectoryService.Shared.Exceptions;

namespace DirectoryService.Presentation.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception e)
        {
            await HandleExceptionAsync(context, e);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception e)
    {
        _logger.LogError(e, e.Message);

        (int statusCode, Error error) = e switch
        {
            NotFoundException ex => (StatusCodes.Status404NotFound, ex.Error),

            BadRequestException ex => (StatusCodes.Status400BadRequest, ex.Error),

            FailureException ex => (StatusCodes.Status500InternalServerError, ex.Error)
        };

        var envelope = Envelope.Error(error);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(envelope);
    }
}