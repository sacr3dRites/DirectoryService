using System.Net;
using System.Net.Http.Json;
using DirectoryService.Application.PaginationUtils;
using DirectoryService.Contracts.Departments;
using DirectoryService.Shared.CustomErrors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Generated;

[Collection(DirectoryIntegrationTestCollection.Name)]
public sealed class DepartmentEndpointTests : DirectoryTestsBase
{
    public DepartmentEndpointTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateDepartment_WithUnknownLocation_ReturnsNotFoundAndDoesNotWrite()
    {
        var request = new CreateDepartmentRequest(
            "Orphan Department",
            "orphan-department",
            [Guid.NewGuid()]);

        using var response = await Client.PostAsJsonAsync("/api/departments", request);

        await AssertErrorEnvelopeAsync(
            response,
            HttpStatusCode.NotFound,
            ErrorType.NOT_FOUND,
            "location.ids.not.found");
        Assert.Equal(0, await CountDepartmentsAsync());
    }

    [Fact]
    public async Task CreateDepartment_WithParent_PersistsParentAndPath()
    {
        var locationId = await CreateLocationThroughApiAsync();
        var parentId = await CreateDepartmentThroughApiAsync(
            [locationId],
            "Parent Department",
            "parent-department");
        var request = new CreateDepartmentRequest(
            "Child Department",
            "child-department",
            [locationId],
            parentId);

        using var response = await Client.PostAsJsonAsync("/api/departments", request);

        var envelope = await AssertSuccessEnvelopeAsync<Guid>(response);
        var childId = AssertValidId(envelope.Result);
        var child = await ExecuteInDbAsync(dbContext =>
            dbContext.Departments
                .AsNoTracking()
                .Include(item => item.Parent)
                .SingleAsync(item => item.Id == childId));

        Assert.Equal(parentId, child.Parent?.Id);
        Assert.Equal("parent-department.child-department", child.Path.Value);
        Assert.Equal(2, await CountDepartmentsAsync());
    }

    [Fact]
    public async Task CreateDepartment_WithUnknownParent_ReturnsNotFoundAndDoesNotWriteDepartment()
    {
        var locationId = await CreateLocationThroughApiAsync();
        var request = new CreateDepartmentRequest(
            "Child Department",
            "child-department",
            [locationId],
            Guid.NewGuid());

        using var response = await Client.PostAsJsonAsync("/api/departments", request);

        await AssertErrorEnvelopeAsync(
            response,
            HttpStatusCode.NotFound,
            ErrorType.NOT_FOUND,
            "record.not.found");
        Assert.Equal(0, await CountDepartmentsAsync());
        Assert.Equal(1, await CountLocationsAsync());
    }

    [Fact]
    public async Task GetDepartment_WhenItExists_ReturnsCompleteEnvelopeBody()
    {
        var locationId = await CreateLocationThroughApiAsync();
        var departmentId = await CreateDepartmentThroughApiAsync(
            [locationId],
            "Finance Department",
            "finance-department");

        using var response = await Client.GetAsync($"/api/departments/{departmentId}");

        var envelope = await AssertSuccessEnvelopeAsync<DepartmentDto>(response);
        var department = Assert.IsType<DepartmentDto>(envelope.Result);

        Assert.Equal(departmentId, department.Id);
        Assert.Equal("Finance Department", department.Name);
        Assert.Equal("finance-department", department.Identifier);
        Assert.Equal("finance-department", department.Path);
        Assert.Null(department.ParentId);
        Assert.True(department.IsActive);
    }

    [Fact]
    public async Task GetDepartment_WhenItDoesNotExist_ReturnsNotFoundEnvelope()
    {
        using var response = await Client.GetAsync($"/api/departments/{Guid.NewGuid()}");

        await AssertErrorEnvelopeAsync(
            response,
            HttpStatusCode.NotFound,
            ErrorType.NOT_FOUND,
            "record.not.found");
    }

    [Fact]
    public async Task UpdateDepartmentLocations_WithValidRequest_ReplacesDatabaseRelations()
    {
        var oldLocationId = await CreateLocationThroughApiAsync("Old Office", "Old Street 1");
        var newLocationId = await CreateLocationThroughApiAsync("New Office", "New Street 2");
        var departmentId = await CreateDepartmentThroughApiAsync([oldLocationId]);

        using var response = await Client.PatchAsJsonAsync(
            $"/api/departments/{departmentId}",
            new UpdateDepartmentLocationsRequest([newLocationId]));

        var envelope = await AssertSuccessEnvelopeAsync<Guid>(response);
        Assert.Equal(departmentId, envelope.Result);

        var persistedLocationIds = await ExecuteInDbAsync(dbContext =>
            dbContext.DepartmentLocations
                .AsNoTracking()
                .Where(item => item.DepartmentId == departmentId)
                .Select(item => item.LocationId)
                .ToArrayAsync());

        Assert.Equal([newLocationId], persistedLocationIds);
    }

    [Fact]
    public async Task UpdateDepartmentLocations_WithUnknownLocation_ReturnsNotFoundAndKeepsOldRelation()
    {
        var oldLocationId = await CreateLocationThroughApiAsync();
        var departmentId = await CreateDepartmentThroughApiAsync([oldLocationId]);

        using var response = await Client.PatchAsJsonAsync(
            $"/api/departments/{departmentId}",
            new UpdateDepartmentLocationsRequest([Guid.NewGuid()]));

        await AssertErrorEnvelopeAsync(
            response,
            HttpStatusCode.NotFound,
            ErrorType.NOT_FOUND,
            "record.not.found");

        var persistedLocationId = await ExecuteInDbAsync(dbContext =>
            dbContext.DepartmentLocations
                .AsNoTracking()
                .Where(item => item.DepartmentId == departmentId)
                .Select(item => item.LocationId)
                .SingleAsync());
        Assert.Equal(oldLocationId, persistedLocationId);
    }

    [Fact]
    public async Task UpdateDepartmentLocations_WithUnknownDepartment_ReturnsNotFoundWithoutRelations()
    {
        var locationId = await CreateLocationThroughApiAsync();

        using var response = await Client.PatchAsJsonAsync(
            $"/api/departments/{Guid.NewGuid()}",
            new UpdateDepartmentLocationsRequest([locationId]));

        await AssertErrorEnvelopeAsync(
            response,
            HttpStatusCode.NotFound,
            ErrorType.NOT_FOUND,
            "record.not.found");
        Assert.Equal(0, await CountDepartmentsAsync());
        var relationCount = await ExecuteInDbAsync(dbContext => dbContext.DepartmentLocations.CountAsync());
        Assert.Equal(0, relationCount);
    }

    [Fact]
    public async Task GetDepartments_WithSearchSortAndPagination_ReturnsExpectedPage()
    {
        var locationId = await CreateLocationThroughApiAsync();
        await CreateDepartmentThroughApiAsync(
            [locationId],
            "Zebra Team",
            "zebra-team");
        var alphaId = await CreateDepartmentThroughApiAsync(
            [locationId],
            "Alpha Team",
            "alpha-team");
        await CreateDepartmentThroughApiAsync(
            [locationId],
            "Beta Unit",
            "beta-unit");

        using var response = await Client.GetAsync(
            "/api/departments?search=Team&sortBy=Name&sortDirection=Asc&page=1&pageSize=1");

        var envelope = await AssertSuccessEnvelopeAsync<PagedResult<DepartmentListItemDto>>(response);
        var page = Assert.IsType<PagedResult<DepartmentListItemDto>>(envelope.Result);
        var item = Assert.Single(page.Items);

        Assert.Equal(1, page.PageNumber);
        Assert.Equal(1, page.PageSize);
        Assert.Equal(2, page.PageCount);
        Assert.Equal(2, page.TotalCount);
        Assert.Equal(alphaId, item.Id);
        Assert.Equal("Alpha Team", item.Name);
    }

    [Fact]
    public async Task DeleteDepartment_WhenItExists_SoftDeletesAndHidesItFromQueries()
    {
        var locationId = await CreateLocationThroughApiAsync();
        var deletedDepartmentId = await CreateDepartmentThroughApiAsync(
            [locationId],
            "Deleted Department",
            "deleted-department");
        var activeDepartmentId = await CreateDepartmentThroughApiAsync(
            [locationId],
            "Active Department",
            "active-department");

        using var deleteResponse = await Client.DeleteAsync($"/api/departments/{deletedDepartmentId}");

        var deleteEnvelope = await AssertSuccessEnvelopeAsync<Guid>(deleteResponse);
        Assert.Equal(deletedDepartmentId, deleteEnvelope.Result);

        var deletedDepartment = await ExecuteInDbAsync(dbContext =>
            dbContext.Departments
                .IgnoreQueryFilters()
                .AsNoTracking()
                .SingleAsync(item => item.Id == deletedDepartmentId));
        Assert.False(deletedDepartment.IsActive);

        using var repeatedDeleteResponse = await Client.DeleteAsync($"/api/departments/{deletedDepartmentId}");
        var repeatedDeleteEnvelope = await AssertSuccessEnvelopeAsync<Guid>(repeatedDeleteResponse);
        Assert.Equal(deletedDepartmentId, repeatedDeleteEnvelope.Result);

        var departmentAfterRepeatedDelete = await ExecuteInDbAsync(dbContext =>
            dbContext.Departments
                .IgnoreQueryFilters()
                .AsNoTracking()
                .SingleAsync(item => item.Id == deletedDepartmentId));
        Assert.Equal(deletedDepartment.UpdatedAt, departmentAfterRepeatedDelete.UpdatedAt);

        using var getResponse = await Client.GetAsync($"/api/departments/{deletedDepartmentId}");
        await AssertErrorEnvelopeAsync(
            getResponse,
            HttpStatusCode.NotFound,
            ErrorType.NOT_FOUND,
            "record.not.found");

        using var listResponse = await Client.GetAsync(
            "/api/departments?sortBy=Name&sortDirection=Asc&page=1&pageSize=20");
        var listEnvelope = await AssertSuccessEnvelopeAsync<PagedResult<DepartmentListItemDto>>(listResponse);
        var page = Assert.IsType<PagedResult<DepartmentListItemDto>>(listEnvelope.Result);

        Assert.Equal(1, page.TotalCount);
        Assert.Equal(activeDepartmentId, Assert.Single(page.Items).Id);
    }
}
