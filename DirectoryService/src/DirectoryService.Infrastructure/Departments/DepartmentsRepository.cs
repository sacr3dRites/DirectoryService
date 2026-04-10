using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Departments;

public class DepartmentsRepository : IDepartmentsRepository
{
    private readonly DirectoryServiceDbContext _context;

    public DepartmentsRepository(DirectoryServiceDbContext dbContext)
    {
        _context = dbContext;
    }

    public Task AddAsync(Department department, CancellationToken cancellationToken = default)
    {
        _context.Departments.AddAsync(department, cancellationToken);
        return _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Department?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Departments.FindAsync(id, cancellationToken);
    }

    public async Task<List<Department>> GetExistingAsync(Guid[] ids, CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }
}