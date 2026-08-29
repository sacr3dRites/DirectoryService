using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.DeleteDepartment;
using DirectoryService.Application.Departments.TransferDepartment;
using DirectoryService.Application.Departments.UpdateDepartmentLocations;
using DirectoryService.Application.PaginationUtils;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared.CustomErrors;
using DirectoryService.Shared.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Departments;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Result<Guid, Errors>, CreateDepartmentCommand> commandHandler,
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateDepartmentCommand(request);

        return await commandHandler.Handle(command, cancellationToken);
    }

    [HttpPatch("{id:guid}")]
    public async Task<EndpointResult<Guid>> UpdateDepartmentLocations(
        [FromServices] ICommandHandler<Result<Guid, Errors>, UpdateDepartmentLocationsCommand> handler,
        [FromBody] UpdateDepartmentLocationsRequest request,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDepartmentLocationsCommand(id, request);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public async Task<EndpointResult<Guid>> TransferDepartment(
        [FromServices] ICommandHandler<Result<Guid, Errors>, TransferDepartmentCommand> handler,
        [FromBody] TransferDepartmentRequest request,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new TransferDepartmentCommand(id, request);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<EndpointResult<DepartmentDto>> Get(
        [FromRoute] Guid id,
        [FromServices] IQueryByIdHandler<DepartmentDto> byIdHandler,
        CancellationToken cancellationToken)
    {
        return await byIdHandler.Handle(id, cancellationToken);
    }

    [HttpGet]
    public async Task<EndpointResult<PagedResult<DepartmentListItemDto>>> GetAllDepartments(
        [FromQuery] GetDepartmentsQuery query,
        [FromServices] IQueryHandler<GetDepartmentsQuery, PagedResult<DepartmentListItemDto>> handler,
        CancellationToken cancellationToken
    )
    {
        return await handler.Handle(query, cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    public async Task<EndpointResult<Guid>> SoftDelete(
        [FromRoute] Guid id,
        [FromServices] ICommandHandler<Result<Guid, Errors>, DeleteDepartmentCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        var command = new DeleteDepartmentCommand(id);

        return await commandHandler.Handle(command, cancellationToken);
    }
}
