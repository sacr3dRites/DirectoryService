using System.Net;
using DirectoryService.Application.PaginationUtils;
using DirectoryService.Contracts.Locations;
using DirectoryService.Shared.CustomErrors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Generated;

[Collection(DirectoryIntegrationTestCollection.Name)]
public sealed class LocationEndpointTests : DirectoryTestsBase
{
    public LocationEndpointTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetLocation_WhenItExists_ReturnsCompleteEnvelopeBody()
    {
        var locationId = await CreateLocationThroughApiAsync("North Office", "North Street 1");

        using var response = await Client.GetAsync($"/api/locations/{locationId}");

        var envelope = await AssertSuccessEnvelopeAsync<LocationDto>(response);
        var location = Assert.IsType<LocationDto>(envelope.Result);

        Assert.Equal(locationId, location.Id);
        Assert.Equal("North Office", location.Name);
        Assert.Equal("North Street 1", location.Address);
        Assert.Equal("UTC", location.Timezone);
        Assert.True(location.IsActive);
    }

    [Fact]
    public async Task GetLocation_WhenItDoesNotExist_ReturnsNotFoundEnvelope()
    {
        using var response = await Client.GetAsync($"/api/locations/{Guid.NewGuid()}");

        await AssertErrorEnvelopeAsync(
            response,
            HttpStatusCode.NotFound,
            ErrorType.NOT_FOUND,
            "record.not.found");
    }

    [Fact]
    public async Task GetLocations_WithFilterAndPagination_ReturnsMatchingPage()
    {
        var busyLocationId = await CreateLocationThroughApiAsync("Alpha Hub", "Alpha Street 1");
        var quietLocationId = await CreateLocationThroughApiAsync("Beta Hub", "Beta Street 2");
        await CreateLocationThroughApiAsync("Gamma Hub", "Gamma Street 3");

        await CreateDepartmentThroughApiAsync(
            [busyLocationId],
            "Alpha Department",
            "alpha-department");
        await CreateDepartmentThroughApiAsync(
            [busyLocationId],
            "Beta Department",
            "beta-department");
        await CreateDepartmentThroughApiAsync(
            [quietLocationId],
            "Gamma Department",
            "gamma-department");

        using var response = await Client.GetAsync(
            "/api/locations?search=Hub&minDepartmentCount=2&sortBy=Name&sortDirection=Asc&page=1&pageSize=1");

        var envelope = await AssertSuccessEnvelopeAsync<PagedResult<LocationListItemDto>>(response);
        var page = Assert.IsType<PagedResult<LocationListItemDto>>(envelope.Result);
        var item = Assert.Single(page.Items);

        Assert.Equal(1, page.PageNumber);
        Assert.Equal(1, page.PageSize);
        Assert.Equal(1, page.PageCount);
        Assert.Equal(1, page.TotalCount);
        Assert.Equal(busyLocationId, item.Id);
        Assert.Equal("Alpha Hub", item.Name);
        Assert.Equal(2, item.DepartmentCount);
    }

    [Fact]
    public async Task GetLocations_WhenFilterMatchesNothing_ReturnsEmptyPageEnvelope()
    {
        await CreateLocationThroughApiAsync("Existing Hub", "Existing Street 1");

        using var response = await Client.GetAsync(
            "/api/locations?search=Missing&minDepartmentCount=0&sortBy=Name&sortDirection=Asc&page=1&pageSize=20");

        var envelope = await AssertSuccessEnvelopeAsync<PagedResult<LocationListItemDto>>(response);
        var page = Assert.IsType<PagedResult<LocationListItemDto>>(envelope.Result);

        Assert.Empty(page.Items);
        Assert.Equal(0, page.PageCount);
        Assert.Equal(0, page.TotalCount);
    }

    [Fact]
    public async Task DeleteLocation_WhenItExists_SoftDeletesAndHidesItFromQueries()
    {
        var deletedLocationId = await CreateLocationThroughApiAsync(
            "Deleted Office",
            "Deleted Street 1");
        var activeLocationId = await CreateLocationThroughApiAsync(
            "Active Office",
            "Active Street 1");

        using var deleteResponse = await Client.DeleteAsync($"/api/locations/{deletedLocationId}");

        var deleteEnvelope = await AssertSuccessEnvelopeAsync<Guid>(deleteResponse);
        Assert.Equal(deletedLocationId, deleteEnvelope.Result);

        var deletedLocation = await ExecuteInDbAsync(dbContext =>
            dbContext.Locations
                .IgnoreQueryFilters()
                .AsNoTracking()
                .SingleAsync(item => item.Id == deletedLocationId));
        Assert.False(deletedLocation.IsActive);

        using var repeatedDeleteResponse = await Client.DeleteAsync($"/api/locations/{deletedLocationId}");
        var repeatedDeleteEnvelope = await AssertSuccessEnvelopeAsync<Guid>(repeatedDeleteResponse);
        Assert.Equal(deletedLocationId, repeatedDeleteEnvelope.Result);

        var locationAfterRepeatedDelete = await ExecuteInDbAsync(dbContext =>
            dbContext.Locations
                .IgnoreQueryFilters()
                .AsNoTracking()
                .SingleAsync(item => item.Id == deletedLocationId));
        Assert.Equal(deletedLocation.UpdatedAt, locationAfterRepeatedDelete.UpdatedAt);

        using var getResponse = await Client.GetAsync($"/api/locations/{deletedLocationId}");
        await AssertErrorEnvelopeAsync(
            getResponse,
            HttpStatusCode.NotFound,
            ErrorType.NOT_FOUND,
            "record.not.found");

        using var listResponse = await Client.GetAsync(
            "/api/locations?sortBy=Name&sortDirection=Asc&page=1&pageSize=20");
        var listEnvelope = await AssertSuccessEnvelopeAsync<PagedResult<LocationListItemDto>>(listResponse);
        var page = Assert.IsType<PagedResult<LocationListItemDto>>(listEnvelope.Result);

        Assert.Equal(1, page.TotalCount);
        Assert.Equal(activeLocationId, Assert.Single(page.Items).Id);
    }
}
