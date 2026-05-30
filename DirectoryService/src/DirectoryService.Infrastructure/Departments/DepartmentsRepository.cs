using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
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
            return Error.Failure("department.add.failed", "Failed to add department" + " " + e.Message);
        }

        return UnitResult.Success<Error>();
    }

    public async Task<Result<IReadOnlyList<Department>, Error>> GetByAsync(
        Expression<Func<Department, bool>> predicate,
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
            return Error.Failure("department.get.failed", "Failed to get departments" + " " + e.Message);
        }
    }

    public async Task<UnitResult<Error>> DeleteLocationsByDepartmentId(
        Guid departmentId,
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
            return Error.Failure("department.delete.failed", "Failed to delete department locations" + " " + e.Message);
        }

        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> AddDepartmentLocations(
        IEnumerable<DepartmentLocation> departmentLocations,
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
            return Error.Failure(
                "department.add.locations.failed",
                "Failed to add department locations" + " " + e.Message);
        }

        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// Изменить parentId у подразделения, пересчитать и обновить Path, Depth.
    /// </summary>
    /// <param name="parentId"> id родителя депа. </param>
    /// <param name="departmentId"> id депа. </param>
    /// <param name="cancellationToken"> cancelation Token. </param>
    /// <returns> Либо ничего, либо ошибку.</returns>
    public async Task<UnitResult<Error>> TransferDepartment(Department? parentDepartment, Department department,
        CancellationToken cancellationToken = default)
    {
        var parentDepth = parentDepartment?.Depth ?? 0;
        var identifier = DepartmentPath.Create(department.Identifier, parentDepartment);
        var depId = department.Id;
        const string sqlQuery = """
                                UPDATE departments
                                SET parent_id = @parent,
                                    depth =  @parentDepth+1,
                                    path = @identifier::ltree
                                WHERE id = @depId
                                """;

        var dbConn = _context.Database.GetDbConnection();

        await dbConn.ExecuteAsync(sqlQuery,
            new { parent = parentDepartment?.Id, parentDepth, identifier = identifier.Value, depId });

        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// Для всех дочерних подразделений и их потомков обновить Path и Depth c использованием ltree
    /// </summary>
    public async Task<UnitResult<Error>> UpdateChildDepartments(
        Guid rootId,
        DepartmentPath oldParentDepartmentPath)
    {
        const string sqlQuery = """
                                WITH root AS (
                                    SELECT id, path, depth
                                    FROM departments
                                    WHERE id = @rootId
                                )
                                UPDATE departments d
                                SET depth = root.depth + (nlevel(d.path) - nlevel(@oldPath::ltree)),
                                    path = root.path || subpath(d.path, nlevel(@oldPath::ltree))
                                FROM root
                                WHERE d.path <@ @oldPath::ltree;
                                """;

        var dbConn = _context.Database.GetDbConnection();

        await dbConn.ExecuteAsync(sqlQuery, new { rootId, oldPath = oldParentDepartmentPath.Value });

        return UnitResult.Success<Error>();
    }

    public async Task<Result<Department, Error>> GetByIdWithLock(Guid commandDepartmentId,
        CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .FromSqlInterpolated(
                $"SELECT id, identifier, name, path, depth, is_active, created_at, updated_at, parent_id FROM departments WHERE id = {commandDepartmentId} AND is_active FOR UPDATE ")
            .FirstOrDefaultAsync(cancellationToken);

        if (department == null)
        {
            return GeneralErrors.NotFound();
        }

        return department;
    }

    public async Task<UnitResult<Error>> LockDescendants(DepartmentPath departmentPath,
        CancellationToken cancellationToken)
    {
        _context.Departments.FromSql($"SELECT * FROM departments WHERE path <@ @departmentPath::ltree FOR UPDATE");

        return UnitResult.Success<Error>();
    }
}