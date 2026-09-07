using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Shared.CustomErrors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.GetAllDepartmentAncestors;

public class
    GetAllDepartmentAncestorsHandler : IQueryByIdHandler<DepartmentTree[]>
{
    private readonly IReadDbContext _context;
    private readonly INpgSqlConnectionFactory _connectionFactory;
    private readonly ILogger<GetAllDepartmentAncestorsHandler> _logger;

    public GetAllDepartmentAncestorsHandler(
        IReadDbContext context,
        INpgSqlConnectionFactory npgSqlConnectionFactory,
        ILogger<GetAllDepartmentAncestorsHandler> logger)
    {
        _context = context;
        _logger = logger;
        _connectionFactory = npgSqlConnectionFactory;
    }

    public async Task<Result<DepartmentTree[], Errors>> Handle(Guid departmentId,
        CancellationToken cancellationToken)
    {
        var dep = await _context.DepartmentsRead.FirstOrDefaultAsync(x => x.Id == departmentId,
            cancellationToken);

        if (dep == null)
        {
            _logger.LogError($"Department with id {departmentId} not found");
            return GeneralErrors.NotFound(departmentId).ToErrors();
        }

        var departmentAncestorListItemQuery = $"""
                                               WITH child_dep AS(
                                               SELECT id, path
                                               FROM departments
                                               WHERE is_active = TRUE AND id = @DepartmentId
                                               )
                                               SELECT 
                                               d.id,
                                               d.name,
                                               d.identifier,
                                               d.path,
                                               d.depth::int as Depth, 
                                               stats.children_count > 0 as HasChildren,
                                               stats.children_count AS ChildrenCount  
                                               FROM departments d
                                               JOIN child_dep target ON d.path @> target.path
                                               CROSS JOIN LATERAL (
                                               SELECT COUNT(*)::int AS children_count
                                               FROM departments child
                                               WHERE child.parent_id = d.id
                                               AND child.is_active = TRUE
                                               ) stats
                                               WHERE d.is_active = TRUE
                                               AND d.id <> target.id
                                               ORDER BY nlevel(d.path) ASC
                                               """;

        using var dbConn = await _connectionFactory.CreateConnectionAsync();

        var parameters = new { DepartmentId = departmentId };

        var command = new CommandDefinition(
            departmentAncestorListItemQuery,
            parameters,
            cancellationToken: cancellationToken);

        var departmentTreesArray =
            (await dbConn.QueryAsync<DepartmentTree>(command)).ToArray();

        return departmentTreesArray;
    }
}