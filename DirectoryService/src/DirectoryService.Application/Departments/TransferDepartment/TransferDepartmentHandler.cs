using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared.CustomErrors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.TransferDepartment;

public class TransferDepartmentHandler : ICommandHandler<Result<Guid, Errors>, TransferDepartmentCommand>
{
    private readonly ILogger<TransferDepartmentHandler> _logger;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IValidator<TransferDepartmentCommand> _validator;
    private readonly ITransactionManager _transactionManager;

    public TransferDepartmentHandler(
        ILogger<TransferDepartmentHandler> logger,
        IDepartmentsRepository departmentsRepository,
        IValidator<TransferDepartmentCommand> validator,
        ITransactionManager transactionManager
    )
    {
        _logger = logger;
        _departmentsRepository = departmentsRepository;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Errors>> Handle(TransferDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        //начало транзакции
        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionResult.IsFailure)
        {
            return transactionResult.Error.ToErrors();
        }

        using var transactionScope = transactionResult.Value;

        //валидация
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            _logger.LogError(validationResult.Errors.First().ErrorMessage);
            return validationResult.ToErrors();
        }

        //Проверить, что существует ли подразделение с таким departmentId и оно активно

        var departmentResult = await _departmentsRepository.GetByIdWithLock(command.DepartmentId, cancellationToken);

        if (departmentResult.IsFailure)
        {
            _logger.LogError(departmentResult.Error.Message);
            return departmentResult.Error.ToErrors();
        }

        var department = departmentResult.Value;

        if (!department.IsActive)
        {
            _logger.LogError("department is not active");
            return GeneralErrors.ValueIsInvalid().ToErrors();
        }

        //блокировка наследников
        var descendantsResult = await _departmentsRepository.LockDescendants(department.Path, cancellationToken);

        if (descendantsResult.IsFailure)
        {
            _logger.LogError(descendantsResult.Error.Message);
            return descendantsResult.Error.ToErrors();
        }

        //Проверить, что новый parentId (если не null) существует, активен и не совпадает с departmentId

        Department? parentDepartment = null;

        if (command.TransferDepartmentRequest.ParentId != null)
        {
            if (department.Parent.Id == command.TransferDepartmentRequest.ParentId)
            {
                _logger.LogError("New parent id is the same as old parent id");
                return GeneralErrors.ValueIsInvalid().ToErrors();
            }

            var parentDepartmentResult = await _departmentsRepository.GetByAsync(
                parentDepartment => parentDepartment.Id.Equals(command.TransferDepartmentRequest.ParentId),
                cancellationToken);

            if (parentDepartmentResult.IsFailure)
            {
                _logger.LogError(parentDepartmentResult.Error.Message);
                return parentDepartmentResult.Error.ToErrors();
            }

            parentDepartment = parentDepartmentResult.Value.FirstOrDefault();

            if (!parentDepartment.IsActive)
            {
                _logger.LogError("Parent department is not active");
                return GeneralErrors.ValueIsInvalid().ToErrors();
            }
        }

        //Нельзя выбрать родителем своё "дочернее" подразделение (чтобы не было зацикливания структуры)
        if (parentDepartment != null)
        {
            if (department.IsAncestorOf(parentDepartment))
            {
                _logger.LogError("Cannot transfer department due to cycle in hierarchy");
                return GeneralErrors.ValueIsInvalid().ToErrors();
            }
        }

        var oldParentDepartmentPath = department.Path;

        //Изменить parentId у подразделения, пересчитать и обновить Path, Depth
        await _departmentsRepository.TransferDepartment(parentDepartment, department, cancellationToken);

        //Для всех дочерних подразделений и их потомков обновить Path и Depth, использовать Ltree
        await _departmentsRepository.UpdateChildDepartments(department.Id, oldParentDepartmentPath);
        //сохранение
        transactionScope.Commit();

        _logger.LogInformation("Department has been transferred");
        return department.Id;
    }
}