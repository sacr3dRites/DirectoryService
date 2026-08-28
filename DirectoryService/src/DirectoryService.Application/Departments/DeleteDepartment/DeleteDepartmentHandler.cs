using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Shared.CustomErrors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.DeleteDepartment;

public class DeleteDepartmentHandler : ICommandHandler<Result<Guid, Errors>, DeleteDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<DeleteDepartmentHandler> _logger;
    private readonly ITransactionManager _transactionManager;

    public DeleteDepartmentHandler(
        ITransactionManager transactionManager,
        IDepartmentsRepository departmentsRepository,
        ILogger<DeleteDepartmentHandler> logger)
    {
        _transactionManager = transactionManager;
        _departmentsRepository = departmentsRepository;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(DeleteDepartmentCommand command, CancellationToken cancellationToken)
    {
        var id = command.Id;

        var depResult = await _departmentsRepository.GetByIncludingInactiveAsync(
            dep => dep.Id == id,
            cancellationToken);

        if (depResult.IsFailure)
        {
            _logger.LogError(depResult.Error.Message);
            return depResult.Error.ToErrors();
        }

        if (!depResult.Value.Any())
        {
            _logger.LogError("No departments found");
            return Error.NotFound("department.not.found", "No departments found").ToErrors();
        }

        var dep = depResult.Value.First();

        if (!dep.IsActive)
        {
            return dep.Id;
        }

        var result = await _departmentsRepository.Delete(dep, false);

        if (result.IsFailure)
        {
            _logger.LogError(result.Error.Message);
            return result.Error.ToErrors();
        }

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveChangesResult.IsFailure)
        {
            _logger.LogError(saveChangesResult.Error.Message);
            return saveChangesResult.Error.ToErrors();
        }

        return dep.Id;
    }
}
