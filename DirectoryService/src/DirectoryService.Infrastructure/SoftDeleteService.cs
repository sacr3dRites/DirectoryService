using DirectoryService.Application.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure;

public class SoftDeleteService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<SoftDeleteService> _logger;
    private readonly SoftDeleteOptions _options;

    public SoftDeleteService(
        IOptions<SoftDeleteOptions> options,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<SoftDeleteService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(_options.SoftDeleteInterval);
        _logger.LogInformation("SoftDelete service started");
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                try
                {
                    _logger.LogInformation("Soft-delete cleanup cycle started");
                    await RunCleanupCycle(cancellationToken);
                }
                catch (OperationCanceledException e)
                    when (cancellationToken.IsCancellationRequested)
                {
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Critical error during SoftDelete service initialization");
                }
            }
        }
        catch (OperationCanceledException e)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(e, "SoftDelete service canceled");
        }
        finally
        {
            _logger.LogInformation("SoftDelete service stopped");
        }
    }

    private async Task RunCleanupCycle(CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow - _options.AgeOfRecords;

        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        var context = scope.ServiceProvider
            .GetRequiredService<DirectoryServiceDbContext>();

        await context.Locations
            .IgnoreQueryFilters()
            .Where(loc => loc.IsActive == false && loc.UpdatedAt <= cutoff)
            .OrderBy(loc => loc.UpdatedAt)
            .Take(_options.BatchSize)
            .ExecuteDeleteAsync(cancellationToken);


        await context.Departments
            .IgnoreQueryFilters()
            .Where(dep =>
                dep.IsActive == false &&
                dep.UpdatedAt <= cutoff &&
                dep.Children.Count == 0)
            .OrderBy(dep => dep.UpdatedAt)
            .Take(_options.BatchSize)
            .ExecuteDeleteAsync(cancellationToken);

        await context.Positions
            .IgnoreQueryFilters()
            .Where(pos => pos.IsActive == false && pos.UpdatedAt <= cutoff)
            .OrderBy(pos => pos.UpdatedAt)
            .Take(_options.BatchSize)
            .ExecuteDeleteAsync(cancellationToken);
    }
}