using DirectoryService.Application.Departments;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions;
using FluentValidation;

namespace DirectoryService.Application.Positions.CreatePosition;

public class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IPositionsRepository _positionsRepository;

    public CreatePositionCommandValidator(IDepartmentsRepository departmentsRepository,
        IPositionsRepository positionsRepository)
    {
        _departmentsRepository = departmentsRepository;
        _positionsRepository = positionsRepository;

        RuleFor(x => x.CreatePositionDto.Name)
            .NotEmpty()
            .MustBeValueObject(CorrectPositionName.Create)
            .Must(name =>
            {
                return _positionsRepository.GetByName(name).Result is null;
            })
            .WithMessage("Active position with this name already exists or name is incorrect");
        ;

        RuleFor(x => x.CreatePositionDto.Description)
            .NotEmpty()
            .MaximumLength(1000)
            .WithMessage("Description cannot be empty or longer than 1000 characters");


        RuleFor(X => X.CreatePositionDto.DepartmentIds)
            .MustAsync(async (departmentIds, cancellationToken) =>
            {
                if (departmentIds == null || !departmentIds.Any())
                    return false;

                if (departmentIds.Count() != departmentIds.Distinct().Count())
                    return false;

                var existingDepartments = await _departmentsRepository.GetExistingAsync(departmentIds);

                var existingIds = existingDepartments
                    .Where(x => x.IsActive)
                    .Select(x => x.Id)
                    .ToHashSet();
                return departmentIds.All(x => existingIds.Contains(x));
            })
            .WithMessage("The list of departments contains non-existent, inactive or duplicate elements");
    }
}