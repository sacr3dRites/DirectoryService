using System.Net;
using System.Net.Http.Json;
using DirectoryService.Contracts.Positions;
using DirectoryService.Shared.CustomErrors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Generated;

[Collection(DirectoryIntegrationTestCollection.Name)]
public sealed class PositionEndpointTests : DirectoryTestsBase
{
    public PositionEndpointTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreatePosition_WithValidRequest_ReturnsEnvelopeAndPersistsRelations()
    {
        var locationId = await CreateLocationThroughApiAsync();
        var firstDepartmentId = await CreateDepartmentThroughApiAsync(
            [locationId],
            "First Department",
            "first-department");
        var secondDepartmentId = await CreateDepartmentThroughApiAsync(
            [locationId],
            "Second Department",
            "second-department");
        var request = new CreatePositionRequest(
            "Senior Developer",
            "Builds Directory Service features",
            [firstDepartmentId, secondDepartmentId]);

        using var response = await Client.PostAsJsonAsync("/api/positions", request);

        var envelope = await AssertSuccessEnvelopeAsync<Guid>(response);
        var positionId = AssertValidId(envelope.Result);

        await ExecuteInDbAsync(async dbContext =>
        {
            var position = await dbContext.Positions
                .AsNoTracking()
                .SingleAsync(item => item.Id == positionId);
            var departmentIds = await dbContext.Set<DirectoryService.Domain.Shared.DepartmentPosition>()
                .AsNoTracking()
                .Where(item => item.PositionId == positionId)
                .Select(item => item.DepartmentId)
                .OrderBy(id => id)
                .ToArrayAsync();

            Assert.Equal(request.Name, position.Name.Value);
            Assert.Equal(request.Description, position.Description);
            Assert.Equal(request.DepartmentIds.OrderBy(id => id), departmentIds);
        });
    }

    [Fact]
    public async Task CreatePosition_WithInvalidName_ReturnsValidationEnvelopeAndDoesNotWrite()
    {
        var request = new CreatePositionRequest("x", "Valid description", []);

        using var response = await Client.PostAsJsonAsync("/api/positions", request);

        await AssertErrorEnvelopeAsync(
            response,
            HttpStatusCode.BadRequest,
            ErrorType.VALIDATION,
            "value.is.invalid");
        Assert.Equal(0, await CountPositionsAsync());
    }

    [Fact]
    public async Task CreatePosition_WithDuplicateDepartmentIds_ReturnsValidationAndDoesNotWrite()
    {
        var locationId = await CreateLocationThroughApiAsync();
        var departmentId = await CreateDepartmentThroughApiAsync([locationId]);
        var request = new CreatePositionRequest(
            "Quality Engineer",
            "Verifies Directory Service",
            [departmentId, departmentId]);

        using var response = await Client.PostAsJsonAsync("/api/positions", request);

        await AssertErrorEnvelopeAsync(
            response,
            HttpStatusCode.BadRequest,
            ErrorType.VALIDATION,
            "values.are.not.distinct");
        Assert.Equal(0, await CountPositionsAsync());
        var relationCount = await ExecuteInDbAsync(dbContext =>
            dbContext.Set<DirectoryService.Domain.Shared.DepartmentPosition>().CountAsync());
        Assert.Equal(0, relationCount);
    }
}
