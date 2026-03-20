using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Contracts.Locations;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Locations;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromServices] ICommandHandler<Result<Guid>, CreateLocationCommand> commandHandler,
        [FromBody] CreateLocationDto request,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateLocationCommand(request);

        var result = await commandHandler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}