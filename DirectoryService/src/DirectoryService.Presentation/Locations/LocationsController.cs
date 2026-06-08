using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Shared.CustomErrors;
using DirectoryService.Shared.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Locations;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Result<Guid, Errors>, CreateLocationCommand> commandHandler,
        [FromBody] CreateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(request);

        return await commandHandler.Handle(command, cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<EndpointResult<LocationDto>> Get(
        [FromRoute] Guid id,
        [FromServices] IQueryByIdHandler<LocationDto> byIdHandler,
        CancellationToken cancellationToken)
    {
        return await byIdHandler.Handle(id, cancellationToken);
    }

    [HttpGet("top")]
    public async Task<EndpointResult<LocationsTopDto[]>> GetTopLocations(
        [FromServices] IQueryHandler<LocationsTopDto[]> handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(cancellationToken);
    }
}