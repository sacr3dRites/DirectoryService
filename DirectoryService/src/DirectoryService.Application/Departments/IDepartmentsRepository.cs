using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Shared;
using DirectoryService.Shared.CustomErrors;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    public Task<UnitResult<Error>> AddAsync(Department department, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<Department>, Error>> GetByAsync(Expression<Func<Department, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> DeleteLocationsByDepartmentId(Guid departmentId,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> AddDepartmentLocations(IEnumerable<DepartmentLocation> departmentLocations,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> TransferDepartment(Department? parent, Department department,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> UpdateChildDepartments(Guid rootId,DepartmentPath oldParentDepartmentPath);
    Task<Result<Department, Error>> GetByIdWithLock(Guid commandDepartmentId, CancellationToken cancellationToken);
    Task<UnitResult<Error>> LockDescendants(DepartmentPath departmentPath, CancellationToken cancellationToken);
}