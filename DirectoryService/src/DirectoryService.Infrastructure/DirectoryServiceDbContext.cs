using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using DirectoryService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure;

public class DirectoryServiceDbContext : DbContext, IReadDbContext
{
    public DirectoryServiceDbContext(DbContextOptions<DirectoryServiceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Location> Locations => Set<Location>();

    public DbSet<DepartmentLocation> DepartmentLocations => Set<DepartmentLocation>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Position> Positions => Set<Position>();

    public IQueryable<Department> DepartmentsRead => Set<Department>().AsNoTracking();

    public IQueryable<Location> LocationsRead => Set<Location>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DirectoryServiceDbContext).Assembly);
    }
}