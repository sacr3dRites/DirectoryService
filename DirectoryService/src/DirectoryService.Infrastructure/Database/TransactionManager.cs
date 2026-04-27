using System.Data;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Shared.CustomErrors;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Database;

public class TransactionManager : ITransactionManager
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<TransactionManager> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public TransactionManager(DirectoryServiceDbContext dbDbContext, ILogger<TransactionManager> logger,
        ILoggerFactory loggerFactory)
    {
        _dbContext = dbDbContext;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    public async Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var logger = _loggerFactory.CreateLogger<TransactionScope>();

            var transactionScope = new TransactionScope(transaction.GetDbTransaction(), logger);
            return transactionScope;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to begin transaction");
            return Error.Failure("database", "Failed to  begin transaction");
        }
    }

    public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to save changes");
            return Error.Failure("database", "Failed to save changes");
        }
    }
}