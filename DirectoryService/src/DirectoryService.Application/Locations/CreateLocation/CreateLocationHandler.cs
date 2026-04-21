using System.Text.Json;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared.CustomErrors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Result<Guid, Errors>, CreateLocationCommand>
{
    private readonly ILocationsRepository _repository;
    private readonly ILogger<CreateLocationHandler> _logger;
    private readonly IValidator<CreateLocationCommand> _validator;
    private readonly ITransactionManager _transactionManager;

    public CreateLocationHandler(ILogger<CreateLocationHandler> logger,
        ILocationsRepository repository,
        IValidator<CreateLocationCommand> validator,
        ITransactionManager transactionManager)
    {
        _logger = logger;
        _validator = validator;
        _repository = repository;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
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

        var name = CorrectLocationName.Create(command.CreateLocationRequest.Name);

        var address = LocationAddress.Create(command.CreateLocationRequest.Address);

        var timezone = Timezone.Create(command.CreateLocationRequest.Timezone);

        var location = Location.Create(
            name.Value,
            address.Value,
            timezone.Value
        );


        await _repository.AddAsync(location, cancellationToken);

        await _transactionManager.SaveChangesAsync(cancellationToken);

        var commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
        {
            return commitedResult.Error.ToErrors();
        }

        _logger.LogInformation($"Created location with id {location.Id}");

        return location.Id;
    }
}