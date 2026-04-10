using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Positions.CreatePosition;
using DirectoryService.Contracts.Positions;
using DirectoryService.Shared.CustomErrors;
using DirectoryService.Shared.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Positions;

[ApiController]
[Route("api/[controller]")]
public class PositionsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromBody] CreatePositionDto request,
        [FromServices] ICommandHandler<Result<Guid, Errors>, CreatePositionCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        var command = new CreatePositionCommand(request);

        return await commandHandler.Handle(command, cancellationToken);
    }
}