using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Shared;
using DirectoryService.Shared.CustomErrors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentHandler : ICommandHandler<Result<Guid, Errors>, CreateDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentRepository;
    private readonly ILogger<CreateDepartmentHandler> _logger;
    private readonly IValidator<CreateDepartmentCommand> _validator;
    private readonly ILocationsRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;

    public CreateDepartmentHandler(
        ILogger<CreateDepartmentHandler> logger,
        ILocationsRepository locationRepository,
        IDepartmentsRepository departmentsRepository,
        IValidator<CreateDepartmentCommand> validator,
        ITransactionManager transactionManager)
    {
        _logger = logger;
        _departmentRepository = departmentsRepository;
        _locationRepository = locationRepository;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error.ToErrors();
        }

        using var transactionScope = transactionScopeResult.Value;

        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            _logger.LogError(validationResult.Errors.First().ErrorMessage);
            return validationResult.ToErrors();
        }

        var locationIds = command.CreateDepartmentRequest.LocationIds;

        if (locationIds.Count() !=
            locationIds.Distinct().Count())
        {
            locationIds = locationIds.Distinct().ToArray();
        }

        var existingLocationsResult =
            await _locationRepository.GetByAsync(
                x => locationIds.Contains(x.Id),
                cancellationToken);

        if (existingLocationsResult.IsFailure)
        {
            _logger.LogError(existingLocationsResult.Error.Message);
            return existingLocationsResult.Error.ToErrors();
        }


        var existingIds = existingLocationsResult.Value
            .Select(x => x.Id)
            .ToHashSet();
        var contains = locationIds.All(x => existingIds.Contains(x));

        if (!contains)
        {
            _logger.LogError("Какие-то id локации были не найдены среди существующих Location");
            return Error.NotFound("location.ids.not.found", "Some of the location ids not found among existing ids")
                .ToErrors();
        }

        var identifier = DepartmentIdentifier.Create(command.CreateDepartmentRequest.Identifier).Value;

        var name = CorrectDepartmentName.Create(command.CreateDepartmentRequest.Name).Value;

        var locationsResult =
            await _locationRepository.GetByAsync(
                x => locationIds.Contains(x.Id),
                cancellationToken);

        if (locationsResult.IsFailure)
        {
            _logger.LogError(locationsResult.Error.Message);
            return locationsResult.Error.ToErrors();
        }

        var locations = locationsResult.Value;

        Department? parent = null;

        if (command.CreateDepartmentRequest.ParentId is Guid parentId)
        {
            var parentResult =
                await _departmentRepository.GetByAsync(department => department.Id == parentId, cancellationToken);

            if (parentResult.IsFailure)
            {
                _logger.LogError(parentResult.Error.Message);
                return parentResult.Error.ToErrors();
            }

            if (!parentResult.Value.Any())
            {
                return GeneralErrors.NotFound(name: "Родительский департамент").ToErrors();
            }

            parent = parentResult.Value.First();
        }

        var departmentResult = Department.Create(identifier, name, parent);

        if (departmentResult.IsFailure)
        {
            _logger.LogError(departmentResult.Error.FirstOrDefault().Message);
            return departmentResult.Error;
        }

        var department = departmentResult.Value;

        var departmentLocations = locations
            .Select(location => DepartmentLocation.Create(location, department))
            .ToList();

        if (departmentLocations.Any(location => location.IsFailure))
        {
            return Error.Validation("department.locations.creation.error",
                "Ошибка во время создания одной из локаций департамента").ToErrors();
        }

        var addDepartmentLocations =
            department.AddDepartmentLocations(departmentLocations.Select(result => result.Value));

        if (addDepartmentLocations.IsFailure)
        {
            _logger.LogError(addDepartmentLocations.Error.Message);
            return addDepartmentLocations.Error.ToErrors();
        }

        await _departmentRepository.AddAsync(department, cancellationToken);
        await _transactionManager.SaveChangesAsync(cancellationToken);

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            return commitResult.Error.ToErrors();
        }

        _logger.LogInformation(
            parent != null
                ? $"Created department with id {department.Id} with parent id {parent.Id}"
                : $"Created department with id {department.Id} with no parent");

        return department.Id;
    }
}