using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.DeleteDepartment;
using DirectoryService.Application.Departments.GetAllDepartmentChildren;
using DirectoryService.Application.Departments.GetAllDepartments;
using DirectoryService.Application.Departments.GetDepartmentTrees;
using DirectoryService.Application.Departments.GetSelectedDepartmentTree;
using DirectoryService.Application.Departments.TransferDepartment;
using DirectoryService.Application.Departments.UpdateDepartmentLocations;
using DirectoryService.Application.PaginationUtils;
using DirectoryService.Contracts.Departments;
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
        [FromQuery] GetDepartmentsRequest request,
        [FromServices] IQueryHandler<GetDepartmentsQuery, PagedResult<DepartmentListItemDto>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetDepartmentsQuery(
            request.Search,
            request.SortBy,
            request.SortDirection,
            request.Page,
            request.PageSize);

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

    [HttpGet("tree")]
    public async Task<EndpointResult<PagedResult<DepartmentTree>>> GetAllRootDepartmentTrees(
        [FromQuery] GetAllRootDepartmentTreesRequest request,
        [FromServices] IQueryHandler<GetAllRootDepartmentTreesQuery, PagedResult<DepartmentTree>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetAllRootDepartmentTreesQuery(
            request.SortBy,
            request.SortDirection,
            request.Page,
            request.PageSize);

        return await handler.Handle(query, cancellationToken);
    }

    [HttpGet("{id:guid}/children")]
    public async Task<EndpointResult<PagedResult<DepartmentTree>>> GetAllDepartmentChildren(
        [FromRoute] Guid id,
        [FromQuery] GetAllDepartmentChildrenRequest request,
        [FromServices] IQueryHandler<GetAllDepartmentChildrenQuery, PagedResult<DepartmentTree>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetAllDepartmentChildrenQuery(
            id,
            request.SortBy,
            request.SortDirection,
            request.Page,
            request.PageSize);

        return await handler.Handle(query, cancellationToken);
    }

    [HttpGet("{id:guid}/ancestors")]
    public async Task<EndpointResult<DepartmentTree[]>> GetAllDepartmentAncestors(
        [FromRoute] Guid id,
        [FromServices] IQueryByIdHandler<DepartmentTree[]> handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(id, cancellationToken);
    }

    [HttpGet("tree/search")]
    public async Task<EndpointResult<PagedResult<DepartmentTree>>> GetSelectedDepartmentTree(
        [FromQuery] GetSelectedDepartmentTreeQuery query,
        [FromServices] IQueryHandler<GetSelectedDepartmentTreeQuery, PagedResult<DepartmentTree>> handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(query, cancellationToken);
    }
}