using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Infrastructure.Departments;
using DirectoryService.Infrastructure.Locations;
using DirectoryService.Infrastructure.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDirectoryService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<DirectoryServiceDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("DirectoryServiceDb");

            IHostEnvironment hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
            ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            options.UseNpgsql(connectionString);

            if (hostEnvironment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }

            options.UseLoggerFactory(loggerFactory);
        });

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IReadDbContext, DirectoryServiceDbContext>();
        services.AddScoped<INpgSqlConnectionFactory, NpgSqlConnectionFactory>();
        services.AddScoped<ILocationsRepository, LocationsRepository>();
        services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();
        services.AddScoped<IPositionsRepository, PositionsesRepository>();
        services.AddScoped<ITransactionManager, TransactionManager>();
        services.AddHostedService<SoftDeleteService>();
        services.AddOptions<SoftDeleteOptions>()
            .Bind(configuration.GetRequiredSection(SoftDeleteOptions.SectionName))
            .Validate(x => x.SoftDeleteInterval > TimeSpan.Zero)
            .Validate(x => x.AgeOfRecords > TimeSpan.Zero)
            .Validate(x => x.BatchSize > 0)
            .ValidateOnStart();

        return services;
    }
}