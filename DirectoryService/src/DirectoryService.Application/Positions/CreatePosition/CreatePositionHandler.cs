using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions;
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
            transactionScope.Rollback();
            return errors;
        }

        var positions = await _positionsRepository.GetByAsync(x => x.Name.Equals(command.CreatePositionRequest.Name));
        if (positions.Any())
        {
            _logger.LogError("Position with name {Name} already exists.", command.CreatePositionRequest.Name);
            transactionScope.Rollback();
            return GeneralErrors.AlreadyExists().ToErrors();
        }

        var departmentIds = command.CreatePositionRequest.DepartmentIds;

        if (departmentIds.Count() !=
            departmentIds.Distinct().Count())
        {
            transactionScope.Rollback();
            return Error.Validation("values.are.not.distinct", "В списке Department Ids есть повторяющиеся Id")
                .ToErrors();
        }

        var existingDepartments =
            await _departmentsRepository.GetByAsync(
                x => departmentIds.Contains(x.Id),
                cancellationToken);

        if (!existingDepartments.All(x => departmentIds.Contains(x.Id)))
        {
            _logger.LogError("Some departments Ids belong to non existent departments.");
            transactionScope.Rollback();
            return GeneralErrors.NotFound(name: "Id некоторых департаментов").ToErrors();
        }

        var name = CorrectPositionName.Create(command.CreatePositionRequest.Name).Value;
        var description = command.CreatePositionRequest.Description;

        var positionResult = Position.Create(name, description, departmentIds);

        if (positionResult.IsFailure)
        {
            _logger.LogError(positionResult.Error.FirstOrDefault().Message);
            transactionScope.Rollback();
            return positionResult.Error;
        }

        var position = positionResult.Value;

        await _positionsRepository.AddAsync(position, cancellationToken);

        await _transactionManager.SaveChangesAsync(cancellationToken);

        var commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
        {
            _logger.LogError(commitedResult.Error.Message);
            transactionScope.Rollback();
            return commitedResult.Error.ToErrors();
        }

        _logger.LogInformation($"Position {name} created");

        return position.Id;
    }
}