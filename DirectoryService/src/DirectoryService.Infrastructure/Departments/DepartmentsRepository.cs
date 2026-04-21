using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
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

    public async Task<List<Department>> GetByAsync(Expression<Func<Department, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    // public async Task<Result<Department, Error>> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    // {
    //     var result = await _context.Departments.FindAsync(id, cancellationToken);
    //     if (result is null)
    //         return Error.NotFound("department.not.found", "Department not found", id);
    //
    //     return result;
    // }
    //
    // public async Task<List<Department>> GetExistingAsync(Guid[] ids, CancellationToken cancellationToken = default)
    // {
    //     return await _context.Departments
    //         .Where(x => ids.Contains(x.Id))
    //         .ToListAsync(cancellationToken);
    // }
}