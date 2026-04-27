using System.Data;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Shared.CustomErrors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Database;

public class TransactionScope : ITransactionScope
{
    private readonly IDbTransaction _transaction;
    private readonly ILogger<TransactionScope> _logger;

    public TransactionScope(IDbTransaction transaction, ILogger<TransactionScope> logger)
    {
        _transaction = transaction;
        _logger = logger;
    }

    public UnitResult<Error> Commit()
    {
        try
        {
            _transaction.Commit();
            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to commit transaction");
            return Error.Failure("transaction.commit.failed", "Failed to commit transaction");
        }
    }

    public UnitResult<Error> Rollback()
    {
        try
        {
            _transaction.Rollback();
            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to rollback transaction");
            return UnitResult.Failure(Error.Failure("rollback.failed", "Failed to rollback transaction"));
        }
    }

    public void Dispose() => _transaction.Dispose();
}