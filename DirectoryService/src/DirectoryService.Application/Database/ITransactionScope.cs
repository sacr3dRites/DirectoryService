using CSharpFunctionalExtensions;
using DirectoryService.Shared.CustomErrors;

namespace DirectoryService.Application.Database;

public interface ITransactionScope : IDisposable
{
    UnitResult<Error> Commit();
    UnitResult<Error> Rollback();
}