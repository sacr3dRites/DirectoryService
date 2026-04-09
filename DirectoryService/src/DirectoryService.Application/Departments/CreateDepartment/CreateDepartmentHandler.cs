using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
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

    public CreateDepartmentHandler(
        ILogger<CreateDepartmentHandler> logger,
        ILocationsRepository locationRepository,
        IDepartmentsRepository departmentsRepository,
        IValidator<CreateDepartmentCommand> validator)
    {
        _logger = logger;
        _departmentRepository = departmentsRepository;
        _locationRepository = locationRepository;
        _validator = validator;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            _logger.LogError(validationResult.Errors.First().ErrorMessage);
            return validationResult.ToErrors();
        }

        var identifier = DepartmentIdentifier.Create(command.CreateDepartmentDto.Identifier).Value;

        var name = CorrectDepartmentName.Create(command.CreateDepartmentDto.Name).Value;

        var locationIds = command.CreateDepartmentDto.LocationIds;
        var locations = await _locationRepository.GetExistingAsync(locationIds, cancellationToken);

        if (command.CreateDepartmentDto.ParentId.HasValue)
        {
            var parentId = command.CreateDepartmentDto.ParentId.Value;
            var parent = _departmentRepository.FindByIdAsync(parentId, cancellationToken)
                .Result;

            var departmentResult = Department.Create(identifier, name, parent, locations);

            if (departmentResult.IsFailure)
            {
                _logger.LogError(departmentResult.Error.FirstOrDefault().Message);
                return departmentResult.Error;
            }

            var department = departmentResult.Value;
            await _departmentRepository.AddAsync(department, cancellationToken);

            _logger.LogInformation($"Created department with id {department.Id} with parent id {parentId}");

            return department.Id;
        }
        else
        {
            var departmentResult = Department.Create(identifier, name, null, locations);

            if (departmentResult.IsFailure)
            {
                _logger.LogError(departmentResult.Error.FirstOrDefault().Message);
                return departmentResult.Error;
            }

            var department = departmentResult.Value;

            await _departmentRepository.AddAsync(department, cancellationToken);

            _logger.LogInformation($"Created department with id {department.Id} with no parent");

            return department.Id;
        }
    }
}