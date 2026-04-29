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

    public async Task AddAsync(Department department, CancellationToken cancellationToken = default)
    {
        await _context.Departments.AddAsync(department, cancellationToken);
    }

    public async Task<IReadOnlyList<Department>> GetByAsync(Expression<Func<Department, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteLocationsByDepartmentId(Guid departmentId, CancellationToken cancellationToken = default)
    {
        await _context.DepartmentLocations
            .Where(l => l.DepartmentId == departmentId)
            .ExecuteDeleteAsync();
    }

    public async Task AddDepartmentLocations(IEnumerable<DepartmentLocation> departmentLocations,
        CancellationToken cancellationToken = default)
    {
        await _context.DepartmentLocations.AddRangeAsync(departmentLocations, cancellationToken);
    }
}