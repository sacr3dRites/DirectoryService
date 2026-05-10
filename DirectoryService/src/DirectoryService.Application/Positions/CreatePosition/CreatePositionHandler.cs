using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using DirectoryService.Shared.CustomErrors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Positions.CreatePosition;

public class CreatePositionHandler : ICommandHandler<Result<Guid, Errors>, CreatePositionCommand>
{
    private readonly IValidator<CreatePositionCommand> _validator;
    private readonly IPositionsRepository _positionsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<CreatePositionHandler> _logger;
    private readonly ITransactionManager _transactionManager;

    public CreatePositionHandler(
        IValidator<CreatePositionCommand> validator,
        IPositionsRepository positionsRepository,
        ILogger<CreatePositionHandler> logger,
        ITransactionManager transactionManager,
        IDepartmentsRepository departmentsRepository)
    {
        _validator = validator;
        _positionsRepository = positionsRepository;
        _logger = logger;
        _transactionManager = transactionManager;
        _departmentsRepository = departmentsRepository;
    }

    public async Task<Result<Guid, Errors>> Handle(CreatePositionCommand command, CancellationToken cancellationToken)
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
            var errors = validationResult.ToErrors();
            _logger.LogError(errors.First().Message);
            return errors;
        }

        var positionsResult =
            await _positionsRepository.GetByAsync(x => x.Name.Equals(command.CreatePositionRequest.Name));

        if (positionsResult.IsFailure)
        {
            _logger.LogError(positionsResult.Error.Message);
            return positionsResult.Error.ToErrors();
        }

        if (positionsResult.Value.Any())
        {
            _logger.LogError("Position with name {Name} already exists.", command.CreatePositionRequest.Name);
            return GeneralErrors.AlreadyExists().ToErrors();
        }

        var departmentIds = command.CreatePositionRequest.DepartmentIds;

        if (departmentIds.Count() !=
            departmentIds.Distinct().Count())
        {
            return Error.Validation("values.are.not.distinct", "В списке Department Ids есть повторяющиеся Id")
                .ToErrors();
        }

        var existingDepartmentsResult =
            await _departmentsRepository.GetByAsync(
                x => departmentIds.Contains(x.Id),
                cancellationToken);

        if (existingDepartmentsResult.IsFailure)
        {
            _logger.LogError(existingDepartmentsResult.Error.Message);
            return existingDepartmentsResult.Error.ToErrors();
        }

        if (!existingDepartmentsResult.Value.All(x => departmentIds.Contains(x.Id)))
        {
            _logger.LogError("Some departments Ids belong to non existent departments.");
            return GeneralErrors.NotFound(name: "Id некоторых департаментов").ToErrors();
        }

        var name = CorrectPositionName.Create(command.CreatePositionRequest.Name).Value;
        var description = command.CreatePositionRequest.Description;

        var positionResult = Position.Create(name, description);

        if (positionResult.IsFailure)
        {
            _logger.LogError(positionResult.Error.FirstOrDefault().Message);
            return positionResult.Error;
        }

        var position = positionResult.Value;

        var departmentPositions = existingDepartmentsResult.Value
            .Select(dep => DepartmentPosition.Create(position, dep))
            .ToList();

        if (departmentPositions.Any(dep => dep.IsFailure))
        {
            return Error.Validation("department.positions.creation.error",
                "Ошибка во время создания одной из позиций департамента").ToErrors();
        }

        var addDepartmentPositions =
            position.AddDepartmentPositions(departmentPositions.Select(result => result.Value));

        if (addDepartmentPositions.IsFailure)
        {
            _logger.LogError(addDepartmentPositions.Error.Message);
            return addDepartmentPositions.Error.ToErrors();
        }

        await _positionsRepository.AddAsync(position, cancellationToken);

        await _transactionManager.SaveChangesAsync(cancellationToken);

        var commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
        {
            _logger.LogError(commitedResult.Error.Message);
            return commitedResult.Error.ToErrors();
        }

        _logger.LogInformation($"Position {name} created");

        return position.Id;
    }
}