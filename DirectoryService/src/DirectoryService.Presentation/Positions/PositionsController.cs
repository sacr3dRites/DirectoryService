using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Positions.CreatePosition;
using DirectoryService.Application.Positions.DeletePosition;
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
        [FromBody] CreatePositionRequest request,
        [FromServices] ICommandHandler<Result<Guid, Errors>, CreatePositionCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        var command = new CreatePositionCommand(request);

        return await commandHandler.Handle(command, cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    public async Task<EndpointResult<Guid>> Delete(
        [FromRoute] Guid id,
        [FromServices] ICommandHandler<Result<Guid, Errors>, DeletePositionCommand> commandHandler,
        CancellationToken cancellationToken
    )
    {
        var command = new DeletePositionCommand(id);
        return await commandHandler.Handle(command, cancellationToken);
    }
}