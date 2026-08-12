using System.Data;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace DirectoryService.Infrastructure.Database;

public class DirectoryServiceDbSeeder
{
    private const int ENTITY_COUNT = 100;
    private const string LOCATION_NAME_PREFIX = "Seed Location ";
    private const string DEPARTMENT_NAME_PREFIX = "Seed Department ";
    private const string DEPARTMENT_IDENTIFIER_PREFIX = "seed_department_";
    private const string POSITION_NAME_PREFIX = "Seed Position ";

    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<DirectoryServiceDbSeeder> _logger;

    public DirectoryServiceDbSeeder(
        DirectoryServiceDbContext dbContext,
        ILogger<DirectoryServiceDbSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var databaseState = await GetDatabaseStateAsync(cancellationToken);

        if (databaseState == DatabaseState.Seeded)
        {
            _logger.LogInformation("The database already contains the complete seed data set.");
            return;
        }

        if (databaseState == DatabaseState.PartiallySeeded)
        {
            throw new InvalidOperationException(
                "Seeding was cancelled because the database contains a partial or modified seed data set. " +
                "Existing data was not changed.");
        }

        var locations = CreateLocations();
        var departments = CreateDepartments();
        var positions = CreatePositions();

        _dbContext.Locations.AddRange(locations);
        _dbContext.Positions.AddRange(positions);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await InsertDepartmentsAsync(departments, transaction, cancellationToken);

        _dbContext.Departments.AttachRange(departments);

        var departmentLocations = LinkDepartmentsToLocations(departments, locations);
        var departmentPositions = LinkDepartmentsToPositions(departments, positions);

        MarkSomeEntitiesAsInactive(locations, departments, positions);

        _dbContext.DepartmentLocations.AddRange(departmentLocations);
        _dbContext.Set<DepartmentPosition>().AddRange(departmentPositions);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Seed completed: {LocationCount} locations, {DepartmentCount} departments, " +
            "{PositionCount} positions, {DepartmentLocationCount} department-location links and " +
            "{DepartmentPositionCount} department-position links.",
            locations.Count,
            departments.Count,
            positions.Count,
            departmentLocations.Count,
            departmentPositions.Count);
    }

    private static List<Location> CreateLocations()
    {
        var locations = new List<Location>(ENTITY_COUNT);

        for (var index = 1; index <= ENTITY_COUNT; index++)
        {
            var suffix = index.ToString("D3");
            var name = CorrectLocationName.Create($"{LOCATION_NAME_PREFIX}{suffix}").Value;
            var address = LocationAddress.Create($"Seed Street {suffix}, Building {index}").Value;
            var timezone = Timezone.Create("UTC").Value;

            locations.Add(Location.Create(name, address, timezone));
        }

        return locations;
    }

    private static List<Department> CreateDepartments()
    {
        var departments = new List<Department>(ENTITY_COUNT);

        for (var index = 1; index <= ENTITY_COUNT; index++)
        {
            var suffix = index.ToString("D3");
            var identifier = DepartmentIdentifier.Create($"{DEPARTMENT_IDENTIFIER_PREFIX}{suffix}").Value;
            var name = CorrectDepartmentName.Create($"{DEPARTMENT_NAME_PREFIX}{suffix}").Value;

            departments.Add(Department.Create(identifier, name, null).Value);
        }

        for (var index = 10; index < ENTITY_COUNT; index++)
        {
            var parentIndex = (index - 10) / 3;
            var addChildResult = departments[parentIndex].AddChildren([departments[index]]);

            if (addChildResult.IsFailure)
            {
                throw new InvalidOperationException(addChildResult.Error);
            }
        }

        return departments;
    }

    private static List<Position> CreatePositions()
    {
        var positions = new List<Position>(ENTITY_COUNT);

        for (var index = 1; index <= ENTITY_COUNT; index++)
        {
            var suffix = index.ToString("D3");
            var name = CorrectPositionName.Create($"{POSITION_NAME_PREFIX}{suffix}").Value;
            var description = $"Seed position {suffix} for pagination and SQL load scenarios.";

            positions.Add(Position.Create(name, description).Value);
        }

        return positions;
    }

    private static async Task<string> GetDepartmentPathTypeAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            """
            SELECT udt_name
            FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = 'departments'
              AND column_name = 'path';
            """,
            connection,
            transaction);

        var pathType = (string?)await command.ExecuteScalarAsync(cancellationToken);

        if (pathType is not("text" or "ltree"))
        {
            throw new InvalidOperationException(
                $"Unsupported departments.path database type: {pathType ?? "not found"}.");
        }

        return pathType;
    }

    private static List<DepartmentLocation> LinkDepartmentsToLocations(
        IReadOnlyList<Department> departments,
        IReadOnlyList<Location> locations)
    {
        var departmentLocations = new List<DepartmentLocation>(ENTITY_COUNT);

        for (var index = 0; index < ENTITY_COUNT; index++)
        {
            var locationIndex = GetLocationIndex(index);
            var departmentLocation = DepartmentLocation.Create(locations[locationIndex], departments[index]).Value;
            var addLocationResult = departments[index].AddDepartmentLocations([departmentLocation]);

            if (addLocationResult.IsFailure)
            {
                throw new InvalidOperationException(addLocationResult.Error.Message);
            }

            departmentLocations.Add(departmentLocation);
        }

        return departmentLocations;
    }

    private static List<DepartmentPosition> LinkDepartmentsToPositions(
        IReadOnlyList<Department> departments,
        IReadOnlyList<Position> positions)
    {
        var departmentPositions = new List<DepartmentPosition>(ENTITY_COUNT);

        for (var index = 0; index < ENTITY_COUNT; index++)
        {
            var positionIndex = GetPositionIndex(index);
            var departmentPosition = DepartmentPosition.Create(positions[positionIndex], departments[index]).Value;
            var addPositionResult = positions[positionIndex].AddDepartmentPositions([departmentPosition]);

            if (addPositionResult.IsFailure)
            {
                throw new InvalidOperationException(addPositionResult.Error.Message);
            }

            departmentPositions.Add(departmentPosition);
        }

        return departmentPositions;
    }

    private static int GetLocationIndex(int departmentIndex) => departmentIndex switch
    {
        < 50 => departmentIndex % 5,
        < 80 => 5 + ((departmentIndex - 50) / 2),
        _ => 20 + (departmentIndex - 80),
    };

    private static int GetPositionIndex(int departmentIndex) => departmentIndex switch
    {
        < 40 => departmentIndex % 4,
        < 80 => 4 + ((departmentIndex - 40) % 16),
        _ => 20 + (departmentIndex - 80),
    };

    private static void MarkSomeEntitiesAsInactive(
        IReadOnlyList<Location> locations,
        IReadOnlyList<Department> departments,
        IReadOnlyList<Position> positions)
    {
        for (var index = 9; index < ENTITY_COUNT; index += 10)
        {
            locations[index].ChangeActiveStatus(false);
            departments[index].ChangeActiveStatus(false);
            positions[index].ChangeActiveStatus(false);
        }
    }

    private async Task InsertDepartmentsAsync(
        IReadOnlyList<Department> departments,
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        var connection = (NpgsqlConnection)_dbContext.Database.GetDbConnection();
        var npgsqlTransaction = (NpgsqlTransaction)transaction.GetDbTransaction();
        var pathType = await GetDepartmentPathTypeAsync(connection, npgsqlTransaction, cancellationToken);
        var pathExpression = pathType == "ltree" ? "@path::ltree" : "@path";

        await using var command = new NpgsqlCommand(
            $"""
             INSERT INTO departments
                 (id, identifier, parent_id, depth, is_active, created_at, updated_at, name, path)
             VALUES
                 (@id, @identifier, @parent_id, @depth, @is_active, @created_at, @updated_at, @name, {pathExpression});
             """,
            connection,
            npgsqlTransaction);

        command.Parameters.Add("id", NpgsqlDbType.Uuid);
        command.Parameters.Add("identifier", NpgsqlDbType.Text);
        command.Parameters.Add("parent_id", NpgsqlDbType.Uuid);
        command.Parameters.Add("depth", NpgsqlDbType.Smallint);
        command.Parameters.Add("is_active", NpgsqlDbType.Boolean);
        command.Parameters.Add("created_at", NpgsqlDbType.TimestampTz);
        command.Parameters.Add("updated_at", NpgsqlDbType.TimestampTz);
        command.Parameters.Add("name", NpgsqlDbType.Text);
        command.Parameters.Add("path", NpgsqlDbType.Text);

        await command.PrepareAsync(cancellationToken);

        foreach (var department in departments)
        {
            command.Parameters["id"].Value = department.Id;
            command.Parameters["identifier"].Value = department.Identifier.Value;
            command.Parameters["parent_id"].Value = department.Parent?.Id ?? (object)DBNull.Value;
            command.Parameters["depth"].Value = department.Depth;
            command.Parameters["is_active"].Value = department.IsActive;
            command.Parameters["created_at"].Value = department.CreatedAt;
            command.Parameters["updated_at"].Value = department.UpdatedAt;
            command.Parameters["name"].Value = department.Name.Value;
            command.Parameters["path"].Value = department.Path.Value;

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private async Task<DatabaseState> GetDatabaseStateAsync(CancellationToken cancellationToken)
    {
        var locations = await _dbContext.Locations.AsNoTracking().ToListAsync(cancellationToken);
        var departments = await _dbContext.Departments.AsNoTracking().ToListAsync(cancellationToken);
        var positions = await _dbContext.Positions.AsNoTracking().ToListAsync(cancellationToken);

        var expectedLocationNames = Enumerable.Range(1, ENTITY_COUNT)
            .Select(index => $"{LOCATION_NAME_PREFIX}{index:D3}")
            .ToHashSet(StringComparer.Ordinal);
        var expectedDepartmentIdentifiers = Enumerable.Range(1, ENTITY_COUNT)
            .Select(index => $"{DEPARTMENT_IDENTIFIER_PREFIX}{index:D3}")
            .ToHashSet(StringComparer.Ordinal);
        var expectedPositionNames = Enumerable.Range(1, ENTITY_COUNT)
            .Select(index => $"{POSITION_NAME_PREFIX}{index:D3}")
            .ToHashSet(StringComparer.Ordinal);

        var seedLocations = locations
            .Where(location => expectedLocationNames.Contains(location.Name.Value))
            .ToList();
        var seedDepartments = departments
            .Where(department => expectedDepartmentIdentifiers.Contains(department.Identifier.Value))
            .ToList();
        var seedPositions = positions
            .Where(position => expectedPositionNames.Contains(position.Name.Value))
            .ToList();

        if (seedLocations.Count == 0 && seedDepartments.Count == 0 && seedPositions.Count == 0)
        {
            return DatabaseState.SeedDataMissing;
        }

        if (seedLocations.Count != ENTITY_COUNT ||
            seedDepartments.Count != ENTITY_COUNT ||
            seedPositions.Count != ENTITY_COUNT)
        {
            return DatabaseState.PartiallySeeded;
        }

        var seedLocationIds = seedLocations.Select(location => location.Id).ToHashSet();
        var seedDepartmentIds = seedDepartments.Select(department => department.Id).ToHashSet();
        var seedPositionIds = seedPositions.Select(position => position.Id).ToHashSet();

        var departmentLocationQuery = _dbContext.DepartmentLocations
            .Where(link => seedDepartmentIds.Contains(link.DepartmentId) &&
                           seedLocationIds.Contains(link.LocationId));
        var departmentPositionQuery = _dbContext.Set<DepartmentPosition>()
            .Where(link => seedDepartmentIds.Contains(link.DepartmentId) &&
                           seedPositionIds.Contains(link.PositionId));

        var departmentLocationCount = await departmentLocationQuery.CountAsync(cancellationToken);
        var departmentsWithLocationsCount = await departmentLocationQuery
            .Select(link => link.DepartmentId)
            .Distinct()
            .CountAsync(cancellationToken);
        var departmentPositionCount = await departmentPositionQuery.CountAsync(cancellationToken);
        var departmentsWithPositionsCount = await departmentPositionQuery
            .Select(link => link.DepartmentId)
            .Distinct()
            .CountAsync(cancellationToken);

        return departmentLocationCount == ENTITY_COUNT &&
               departmentsWithLocationsCount == ENTITY_COUNT &&
               departmentPositionCount == ENTITY_COUNT &&
               departmentsWithPositionsCount == ENTITY_COUNT
            ? DatabaseState.Seeded
            : DatabaseState.PartiallySeeded;
    }

    private enum DatabaseState
    {
        SeedDataMissing,
        Seeded,
        PartiallySeeded,
    }
}
