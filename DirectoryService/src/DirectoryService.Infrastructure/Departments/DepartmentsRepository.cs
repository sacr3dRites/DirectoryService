using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using DirectoryService.Shared.CustomErrors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Departments;

public class DepartmentsRepository : IDepartmentsRepository
{
    private readonly DirectoryServiceDbContext _context;

    public DepartmentsRepository(DirectoryServiceDbContext dbContext)
    {
        _context = dbContext;
    }

    public async Task<UnitResult<Error>> AddAsync(Department department, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Departments.AddAsync(department, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            return Error.Failure("department.add.failed", "Failed to add department");
        }

        return UnitResult.Success<Error>();
    }

    public async Task<Result<IReadOnlyList<Department>, Error>> GetByAsync(Expression<Func<Department, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Departments
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            return Error.Failure("department.get.failed", "Failed to get departments");
        }
    }

    public async Task<UnitResult<Error>> DeleteLocationsByDepartmentId(Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.DepartmentLocations
                .Where(l => l.DepartmentId == departmentId)
                .ExecuteDeleteAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            return Error.Failure("department.delete.failed", "Failed to delete department locations");
        }

        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> AddDepartmentLocations(IEnumerable<DepartmentLocation> departmentLocations,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.DepartmentLocations.AddRangeAsync(departmentLocations, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            return Error.Failure("department.add.locations.failed", "Failed to add department locations");
        }

        return UnitResult.Success<Error>();
    }
}