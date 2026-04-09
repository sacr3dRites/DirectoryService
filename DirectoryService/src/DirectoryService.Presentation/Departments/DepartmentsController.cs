using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.CreateDepartment;
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
        [FromBody] CreateDepartmentDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateDepartmentCommand(request);

        return await commandHandler.Handle(command, cancellationToken);
    }
}