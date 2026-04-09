using DirectoryService.Domain.Departments;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    public Task AddAsync(Department department, CancellationToken cancellationToken = default);
    
    public Task<Department?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
}