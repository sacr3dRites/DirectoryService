using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared.CustomErrors;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    public Task AddAsync(Department department, CancellationToken cancellationToken = default);

    Task<List<Department>> GetByAsync(Expression<Func<Department, bool>> predicate,
        CancellationToken cancellationToken = default);
}