using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure;

public class DirectoryServiceDbContext : DbContext
{
    public DirectoryServiceDbContext(DbContextOptions<DirectoryServiceDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DirectoryServiceDbContext).Assembly);
    }
}