using DirectoryService.Contracts.Locations;

namespace DirectoryService.IntegrationTests.Generated;

[Collection(DirectoryIntegrationTestCollection.Name)]
public sealed class TopLocationsEndpointTests : DirectoryTestsBase
{
    public TopLocationsEndpointTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetTopLocations_ReturnsOnlyLocationsWithAtLeastFiveDepartments()
    {
        var topLocationId = await CreateLocationThroughApiAsync("Top Hub", "Top Street 1");
        var regularLocationId = await CreateLocationThroughApiAsync("Regular Hub", "Regular Street 2");

        for (var index = 1; index <= 5; index++)
        {
            await CreateDepartmentThroughApiAsync(
                [topLocationId],
                $"Top Department {index}",
                $"top-department-{index}");
        }

        await CreateDepartmentThroughApiAsync(
            [regularLocationId],
            "Regular Department",
            "regular-department");

        using var response = await Client.GetAsync("/api/locations/top");

        var envelope = await AssertSuccessEnvelopeAsync<LocationsTopDto[]>(response);
        var location = Assert.Single(Assert.IsType<LocationsTopDto[]>(envelope.Result));

        Assert.Equal(topLocationId, location.Id);
        Assert.Equal("Top Hub", location.LocationName);
        Assert.Equal("Top Street 1", location.Address);
        Assert.Equal(5, location.DepartmentCount);
    }

    [Fact]
    public async Task GetTopLocations_WithCleanDatabase_ReturnsEmptyEnvelopeResult()
    {
        using var response = await Client.GetAsync("/api/locations/top");

        var envelope = await AssertSuccessEnvelopeAsync<LocationsTopDto[]>(response);

        Assert.Empty(Assert.IsType<LocationsTopDto[]>(envelope.Result));
    }
}
