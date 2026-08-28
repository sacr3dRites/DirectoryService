using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Shared.CustomErrors;
using Microsoft.EntityFrameworkCore;
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
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            _logger.LogError(transactionScopeResult.Error.Message);
            return transactionScopeResult.Error.ToErrors();
        }

        using var transactionScope = transactionScopeResult.Value;

        var id = command.Id;

        var depResult = await _departmentsRepository.GetByAsync(dep => dep.Id == id);

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

        var result = await _departmentsRepository.Delete(dep, false);

        if (result.IsFailure)
        {
            _logger.LogError(result.Error.Message);
            return result.Error.ToErrors();
        }

        await _transactionManager.SaveChangesAsync(cancellationToken);

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            return commitResult.Error.ToErrors();
        }

        return dep.Id;
    }
}