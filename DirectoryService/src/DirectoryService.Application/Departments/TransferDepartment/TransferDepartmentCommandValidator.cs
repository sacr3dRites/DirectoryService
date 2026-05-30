using FluentValidation;

namespace DirectoryService.Application.Departments.TransferDepartment;

public class TransferDepartmentCommandValidator : AbstractValidator<TransferDepartmentCommand>
{
    public TransferDepartmentCommandValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty();
    }
}