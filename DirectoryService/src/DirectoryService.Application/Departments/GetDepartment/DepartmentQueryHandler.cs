using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Shared.CustomErrors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Departments.GetDepartment;

public class DepartmentQueryHandler : IQueryHandler<DepartmentDto>
{
    private readonly IReadDbContext _context;

    public DepartmentQueryHandler(IReadDbContext readDbContext)
    {
        _context = readDbContext;
    }

    public async Task<Result<DepartmentDto, Errors>> Handle(Guid id, CancellationToken cancellationToken)
    {
        var dep = await _context.DepartmentsRead
            .Include(d => d.Parent)
            .FirstOrDefaultAsync(department => department.Id == id, cancellationToken);

        if (dep == null)
        {
            return GeneralErrors.NotFound().ToErrors();
        }

        return new DepartmentDto
        (
            dep.Id,
            dep.Name.Value,
            dep.Identifier.Value,
            dep.Path.Value,
            dep.Parent?.Id ?? null,
            dep.Depth,
            dep.IsActive,
            dep.CreatedAt,
            dep.UpdatedAt
        );
    }
}