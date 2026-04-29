using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using DirectoryService.Shared.CustomErrors;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    public Task AddAsync(Department department, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Department>> GetByAsync(Expression<Func<Department, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task DeleteLocationsByDepartmentId(Guid departmentId, CancellationToken cancellationToken = default);

    Task AddDepartmentLocations(IEnumerable<DepartmentLocation> departmentLocations,
        CancellationToken cancellationToken = default);
}