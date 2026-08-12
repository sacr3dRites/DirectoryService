using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Seeder;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory,
        });

        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(options => options.SingleLine = true);

        builder.Services.AddDirectoryService(builder.Configuration);
        builder.Services.AddScoped<DirectoryServiceDbSeeder>();

        using var host = builder.Build();
        await using var scope = host.Services.CreateAsyncScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbSeeder>();
            await seeder.SeedAsync();
            return 0;
        }
        catch (Exception exception)
        {
            logger.LogCritical(
                exception,
                "Database seeding failed. Make sure PostgreSQL is available and all migrations have already been applied.");
            return 1;
        }
    }
}
