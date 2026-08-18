using System.Net;
using System.Net.Http.Json;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Shared.CustomErrors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests;

// These tests were written manually and are the reference style for generated scenarios.
[Collection(DirectoryIntegrationTestCollection.Name)]
public sealed class ManualReferenceTests : DirectoryTestsBase
{
    public ManualReferenceTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateLocation_WithValidRequest_ReturnsEnvelopeAndPersistsLocation()
    {
        var request = new CreateLocationRequest(
            "Main Office",
            "Main Street 10",
            "UTC");

        using var response = await Client.PostAsJsonAsync("/api/locations", request);

        var envelope = await AssertSuccessEnvelopeAsync<Guid>(response);
        var locationId = AssertValidId(envelope.Result);

        var location = await ExecuteInDbAsync(dbContext =>
            dbContext.Locations.AsNoTracking().SingleAsync(item => item.Id == locationId));

        Assert.Equal(request.Name, location.Name.Value);
        Assert.Equal(request.Address, location.LocationAddress.Value);
        Assert.Equal(request.Timezone, location.Timezone.Name);
        Assert.True(location.IsActive);
    }

    [Fact]
    public async Task CreateDepartment_WithValidRequest_ReturnsEnvelopeAndPersistsRelations()
    {
        var locationId = await CreateLocationThroughApiAsync();
        var request = new CreateDepartmentRequest(
            "Platform Department",
            "platform-department",
            [locationId]);

        using var response = await Client.PostAsJsonAsync("/api/departments", request);

        var envelope = await AssertSuccessEnvelopeAsync<Guid>(response);
        var departmentId = AssertValidId(envelope.Result);

        await ExecuteInDbAsync(async dbContext =>
        {
            var department = await dbContext.Departments
                .AsNoTracking()
                .SingleAsync(item => item.Id == departmentId);
            var relation = await dbContext.DepartmentLocations
                .AsNoTracking()
                .SingleAsync(item => item.DepartmentId == departmentId);

            Assert.Equal(request.Name, department.Name.Value);
            Assert.Equal(request.Identifier, department.Identifier.Value);
            Assert.Equal(request.Identifier, department.Path.Value);
            Assert.Equal(locationId, relation.LocationId);
        });
    }

    [Fact]
    public async Task CreateLocation_WithInvalidName_ReturnsValidationEnvelopeAndDoesNotWrite()
    {
        var request = new CreateLocationRequest("x", "Valid Street 20", "UTC");

        using var response = await Client.PostAsJsonAsync("/api/locations", request);

        await AssertErrorEnvelopeAsync(
            response,
            HttpStatusCode.BadRequest,
            ErrorType.VALIDATION,
            "value.is.invalid");
        Assert.Equal(0, await CountLocationsAsync());
    }
}
