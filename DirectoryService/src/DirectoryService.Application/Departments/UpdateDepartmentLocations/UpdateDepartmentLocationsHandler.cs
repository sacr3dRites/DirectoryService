using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Shared;
using DirectoryService.Shared.CustomErrors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.UpdateDepartmentLocations;

public class UpdateDepartmentLocationsHandler : ICommandHandler<Result<Guid, Errors>, UpdateDepartmentLocationsCommand>
{
    private readonly IDepartmentsRepository _departmentRepository;
    private readonly ILogger<UpdateDepartmentLocationsHandler> _logger;
    private readonly IValidator<UpdateDepartmentLocationsCommand> _validator;
    private readonly ITransactionManager _transactionManager;
    private readonly ILocationsRepository _locationRepository;

    public UpdateDepartmentLocationsHandler(
        ILogger<UpdateDepartmentLocationsHandler> logger,
        IDepartmentsRepository departmentRepository,
        ILocationsRepository locationsRepository,
        IValidator<UpdateDepartmentLocationsCommand> validator,
        ITransactionManager transactionManager)
    {
        _logger = logger;
        _departmentRepository = departmentRepository;
        _validator = validator;
        _transactionManager = transactionManager;
        _locationRepository = locationsRepository;
    }

    public async Task<Result<Guid, Errors>> Handle(
        UpdateDepartmentLocationsCommand command,
        CancellationToken cancellationToken)
    {
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            _logger.LogError(transactionScopeResult.Error.Message);
            return transactionScopeResult.Error.ToErrors();
        }

        var transactionScope = transactionScopeResult.Value;

        var validatorResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validatorResult.IsValid)
        {
            return validatorResult.ToErrors();
        }


        // Проверить — существует ли подразделение с таким departmentId и оно активно

        var department =
            (await _departmentRepository.GetByAsync(dep => dep.Id.Equals(command.DepartmentId), cancellationToken))
            .FirstOrDefault();

        if (department == null)
        {
            _logger.LogError($"Department with id {command.DepartmentId} not found");
            return GeneralErrors.NotFound(id: command.DepartmentId, "department id").ToErrors();
        }

        // Проверить — все locationIds существуют и активны, нет дубликатов
        var locationIds = command.UpdateDepartmentLocationsRequest.LocationIds;

        if (locationIds.Count() !=
            locationIds.Distinct().Count())
        {
            locationIds = locationIds.Distinct().ToArray();
        }

        var locations =
            await _locationRepository.GetByAsync(
                location => locationIds.Contains(location.Id) && location.IsActive,
                cancellationToken);

        var existingLocations = locations.Distinct();

        if (!existingLocations.Any())
        {
            _logger.LogError($"Locations not found");
            return GeneralErrors.NotFound().ToErrors();
        }

        var departmentLocations = existingLocations
            .Select(location => DepartmentLocation.Create(location, department))
            .ToList();

        if (departmentLocations.Any(location => location.IsFailure))
        {
            return Error.Validation("department.locations.creation.error",
                "Ошибка во время создания одной из локаций департамента").ToErrors();
        }

        // Обновить — заменить старые привязки к локациям новым списком
        await _departmentRepository.DeleteLocationsByDepartmentId(department.Id, cancellationToken);

        await _departmentRepository.AddDepartmentLocations(departmentLocations.Select(result => result.Value));

        await _transactionManager.SaveChangesAsync(cancellationToken);

        var commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
        {
            return commitedResult.Error.ToErrors();
        }

        _logger.LogInformation($"Department with id {department.Id} updated");
        return department.Id;
    }
}