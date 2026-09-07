using System.Net;
using DirectoryService.Application.PaginationUtils;
using DirectoryService.Contracts.Departments;
using DirectoryService.Shared.CustomErrors;

namespace DirectoryService.IntegrationTests.Generated;

[Collection(DirectoryIntegrationTestCollection.Name)]
public sealed class DepartmentTreeEndpointTests : DirectoryTestsBase
{
    public DepartmentTreeEndpointTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetTree_WithEmptyDatabase_ReturnsEmptyPage()
    {
        using var response = await Client.GetAsync("/api/departments/tree?page=1&pageSize=20");

        var page = await AssertTreePageAsync(response);

        Assert.Empty(page.Items);
        AssertPageMetadata(page, pageNumber: 1, pageSize: 20, totalCount: 0, pageCount: 0);
    }

    [Fact]
    public async Task GetTree_ReturnsOnlyActiveRootsAndCountsOnlyActiveDirectChildren()
    {
        var locationId = await CreateLocationThroughApiAsync();
        var rootId = await CreateNodeAsync(locationId, "Alpha Root", "alpha_root");
        var leafRootId = await CreateNodeAsync(locationId, "Zulu Root", "zulu_root");
        var childId = await CreateNodeAsync(locationId, "Active Child", "active_child", rootId);
        await CreateNodeAsync(locationId, "Grandchild", "grandchild", childId);
        var deletedChildId = await CreateNodeAsync(locationId, "Deleted Child", "deleted_child", rootId);
        var deletedRootId = await CreateNodeAsync(locationId, "Deleted Root", "deleted_root");
        await CreateNodeAsync(locationId, "Orphan Child", "orphan_child", deletedRootId);
        await DeleteNodeAsync(deletedChildId);
        await DeleteNodeAsync(deletedRootId);

        using var response = await Client.GetAsync(
            "/api/departments/tree?sortBy=Name&sortDirection=Asc&page=1&pageSize=20");

        var page = await AssertTreePageAsync(response);

        AssertPageMetadata(page, pageNumber: 1, pageSize: 20, totalCount: 2, pageCount: 1);
        Assert.Collection(
            page.Items,
            root => Assert.Equal(
                new DepartmentTree(rootId, "Alpha Root", "alpha_root", "alpha_root", 0, true, 1),
                root),
            root => Assert.Equal(
                new DepartmentTree(leafRootId, "Zulu Root", "zulu_root", "zulu_root", 0, false, 0),
                root));
    }

    [Theory]
    [InlineData("Asc", "Alpha Root", "Beta Root")]
    [InlineData("Desc", "Zulu Root", "Beta Root")]
    public async Task GetTree_WithNameSortAndPagination_ReturnsExpectedRootPage(
        string direction,
        string firstName,
        string secondName)
    {
        var locationId = await CreateLocationThroughApiAsync();
        await CreateNodeAsync(locationId, "Zulu Root", "zulu_root");
        var rootId = await CreateNodeAsync(locationId, "Alpha Root", "alpha_root");
        await CreateNodeAsync(locationId, "Beta Root", "beta_root");
        await CreateNodeAsync(locationId, "Child Root Match", "child_match", rootId);

        using var response = await Client.GetAsync(
            $"/api/departments/tree?sortBy=Name&sortDirection={direction}&page=1&pageSize=2");

        var page = await AssertTreePageAsync(response);

        Assert.Equal(new[] { firstName, secondName }, page.Items.Select(item => item.Name).ToArray());
        AssertPageMetadata(page, pageNumber: 1, pageSize: 2, totalCount: 3, pageCount: 2);
    }

    [Fact]
    public async Task GetChildren_ForLeaf_ReturnsEmptyPage()
    {
        var locationId = await CreateLocationThroughApiAsync();
        var leafId = await CreateNodeAsync(locationId, "Leaf Department", "leaf_department");

        using var response = await Client.GetAsync($"/api/departments/{leafId}/children?page=1&pageSize=20");

        var page = await AssertTreePageAsync(response);

        Assert.Empty(page.Items);
        AssertPageMetadata(page, pageNumber: 1, pageSize: 20, totalCount: 0, pageCount: 0);
    }

    [Fact]
    public async Task GetChildren_ReturnsOnlyActiveDirectChildrenWithChildStatistics()
    {
        var locationId = await CreateLocationThroughApiAsync();
        var rootId = await CreateNodeAsync(locationId, "Root Department", "root_department");
        var alphaId = await CreateNodeAsync(locationId, "Alpha Child", "alpha_child", rootId);
        var zuluId = await CreateNodeAsync(locationId, "Zulu Child", "zulu_child", rootId);
        await CreateNodeAsync(locationId, "Active Grandchild", "active_grandchild", alphaId);
        var deletedGrandchildId = await CreateNodeAsync(
            locationId, "Deleted Grandchild", "deleted_grandchild", alphaId);
        var deletedChildId = await CreateNodeAsync(locationId, "Deleted Child", "deleted_child", rootId);
        await CreateNodeAsync(locationId, "Unrelated Department", "unrelated_department");
        await DeleteNodeAsync(deletedGrandchildId);
        await DeleteNodeAsync(deletedChildId);

        using var response = await Client.GetAsync(
            $"/api/departments/{rootId}/children?sortBy=Name&sortDirection=Asc&page=1&pageSize=20");

        var page = await AssertTreePageAsync(response);

        AssertPageMetadata(page, pageNumber: 1, pageSize: 20, totalCount: 2, pageCount: 1);
        Assert.Collection(
            page.Items,
            child => Assert.Equal(
                new DepartmentTree(alphaId, "Alpha Child", "alpha_child", "root_department.alpha_child", 1, true, 1),
                child),
            child => Assert.Equal(
                new DepartmentTree(zuluId, "Zulu Child", "zulu_child", "root_department.zulu_child", 1, false, 0),
                child));
    }

    [Theory]
    [InlineData("children")]
    [InlineData("ancestors")]
    public async Task GetNodeHierarchy_ForUnknownNode_ReturnsNotFound(string endpoint)
    {
        using var response = await Client.GetAsync($"/api/departments/{Guid.NewGuid()}/{endpoint}");

        await AssertErrorEnvelopeAsync(
            response, HttpStatusCode.NotFound, ErrorType.NOT_FOUND, "record.not.found");
    }

    [Theory]
    [InlineData("children")]
    [InlineData("ancestors")]
    public async Task GetNodeHierarchy_ForDeletedNode_ReturnsNotFound(string endpoint)
    {
        var locationId = await CreateLocationThroughApiAsync();
        var nodeId = await CreateNodeAsync(locationId, "Deleted Department", "deleted_department");
        await DeleteNodeAsync(nodeId);

        using var response = await Client.GetAsync($"/api/departments/{nodeId}/{endpoint}");

        await AssertErrorEnvelopeAsync(
            response, HttpStatusCode.NotFound, ErrorType.NOT_FOUND, "record.not.found");
    }

    [Fact]
    public async Task GetAncestors_ForRoot_ReturnsEmptyArray()
    {
        var locationId = await CreateLocationThroughApiAsync();
        var rootId = await CreateNodeAsync(locationId, "Root Department", "root_department");

        using var response = await Client.GetAsync($"/api/departments/{rootId}/ancestors");

        var envelope = await AssertSuccessEnvelopeAsync<DepartmentTree[]>(response);

        Assert.Empty(Assert.IsType<DepartmentTree[]>(envelope.Result));
    }

    [Fact]
    public async Task GetAncestors_ReturnsRootToParentAndExcludesTargetAndOtherBranches()
    {
        var locationId = await CreateLocationThroughApiAsync();
        var rootId = await CreateNodeAsync(locationId, "Zulu Root", "root_department");
        var parentId = await CreateNodeAsync(locationId, "Alpha Parent", "parent_department", rootId);
        var targetId = await CreateNodeAsync(locationId, "Middle Target", "target_department", parentId);
        await CreateNodeAsync(locationId, "Other Branch", "other_branch", rootId);
        await CreateNodeAsync(locationId, "Target Child", "target_child", targetId);

        using var response = await Client.GetAsync($"/api/departments/{targetId}/ancestors");

        var envelope = await AssertSuccessEnvelopeAsync<DepartmentTree[]>(response);
        var ancestors = Assert.IsType<DepartmentTree[]>(envelope.Result);

        Assert.Collection(
            ancestors,
            node => Assert.Equal(
                new DepartmentTree(rootId, "Zulu Root", "root_department", "root_department", 0, true, 2),
                node),
            node => Assert.Equal(
                new DepartmentTree(
                    parentId, "Alpha Parent", "parent_department", "root_department.parent_department", 1, true, 1),
                node));
    }

    [Fact]
    public async Task GetAncestors_ExcludesSoftDeletedAncestors()
    {
        var locationId = await CreateLocationThroughApiAsync();
        var rootId = await CreateNodeAsync(locationId, "Root Department", "root_department");
        var parentId = await CreateNodeAsync(locationId, "Deleted Parent", "deleted_parent", rootId);
        var targetId = await CreateNodeAsync(locationId, "Target Department", "target_department", parentId);
        await DeleteNodeAsync(parentId);

        using var response = await Client.GetAsync($"/api/departments/{targetId}/ancestors");

        var envelope = await AssertSuccessEnvelopeAsync<DepartmentTree[]>(response);
        var root = Assert.Single(Assert.IsType<DepartmentTree[]>(envelope.Result));

        Assert.Equal(rootId, root.Id);
        Assert.False(root.HasChildren);
        Assert.Equal(0, root.ChildrenCount);
    }

    [Fact]
    public async Task SearchTree_WhenNoNamesMatch_ReturnsEmptyPage()
    {
        var locationId = await CreateLocationThroughApiAsync();
        await CreateNodeAsync(locationId, "Engineering Department", "finance_identifier");

        using var response = await Client.GetAsync(
            "/api/departments/tree/search?search=Finance&page=1&pageSize=5");

        var page = await AssertTreePageAsync(response);

        Assert.Empty(page.Items);
        AssertPageMetadata(page, pageNumber: 1, pageSize: 5, totalCount: 0, pageCount: 0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("x")]
    [InlineData(" x ")]
    public async Task SearchTree_WithMissingOrTooShortSearch_ReturnsBadRequest(string? search)
    {
        var url = search is null
            ? "/api/departments/tree/search?page=1&pageSize=20"
            : $"/api/departments/tree/search?search={Uri.EscapeDataString(search)}&page=1&pageSize=20";

        using var response = await Client.GetAsync(url);

        await AssertErrorEnvelopeAsync(
            response, HttpStatusCode.BadRequest, ErrorType.VALIDATION, "value.is.invalid");
    }

    [Theory]
    [InlineData("finance")]
    [InlineData("FINANCE")]
    [InlineData("  FiNaNcE  ")]
    [InlineData("fi")]
    public async Task SearchTree_MatchesOnlyActiveNamesIgnoringCaseAndReturnsBranchPath(string search)
    {
        var locationId = await CreateLocationThroughApiAsync();
        var rootId = await CreateNodeAsync(locationId, "Company Department", "company_department");
        var matchId = await CreateNodeAsync(locationId, "Finance Department", "budget_department", rootId);
        await CreateNodeAsync(locationId, "Accounts Department", "accounts_department", matchId);
        await CreateNodeAsync(locationId, "Engineering Department", "finance_identifier");
        var deletedId = await CreateNodeAsync(locationId, "Finance Deleted", "deleted_department", rootId);
        await DeleteNodeAsync(deletedId);

        using var response = await Client.GetAsync(
            $"/api/departments/tree/search?search={Uri.EscapeDataString(search)}&page=1&pageSize=5");

        var page = await AssertTreePageAsync(response);
        var match = Assert.Single(page.Items);

        Assert.Equal(
            new DepartmentTree(
                matchId, "Finance Department", "budget_department", "company_department.budget_department", 1, true, 1),
            match);
        AssertPageMetadata(page, pageNumber: 1, pageSize: 5, totalCount: 1, pageCount: 1);
    }

    [Fact]
    public async Task SearchTree_WithPagination_ReturnsNameOrderedPageAndTotalAcrossAllMatches()
    {
        var locationId = await CreateLocationThroughApiAsync();
        await CreateNodeAsync(locationId, "Zulu Finance", "zulu_department");
        await CreateNodeAsync(locationId, "Alpha Finance", "alpha_department");
        var betaId = await CreateNodeAsync(locationId, "Beta Finance", "beta_department");
        await CreateNodeAsync(locationId, "Engineering Department", "engineering_department");

        using var response = await Client.GetAsync(
            "/api/departments/tree/search?search=finance&page=2&pageSize=1");

        var page = await AssertTreePageAsync(response);
        var match = Assert.Single(page.Items);

        Assert.Equal(betaId, match.Id);
        Assert.Equal("Beta Finance", match.Name);
        AssertPageMetadata(page, pageNumber: 2, pageSize: 1, totalCount: 3, pageCount: 3);
    }

    private Task<Guid> CreateNodeAsync(Guid locationId, string name, string identifier, Guid? parentId = null) =>
        CreateDepartmentThroughApiAsync([locationId], name, identifier, parentId);

    private async Task DeleteNodeAsync(Guid id)
    {
        using var response = await Client.DeleteAsync($"/api/departments/{id}");
        var envelope = await AssertSuccessEnvelopeAsync<Guid>(response);
        Assert.Equal(id, envelope.Result);
    }

    private async Task<PagedResult<DepartmentTree>> AssertTreePageAsync(HttpResponseMessage response)
    {
        var envelope = await AssertSuccessEnvelopeAsync<PagedResult<DepartmentTree>>(response);
        return Assert.IsType<PagedResult<DepartmentTree>>(envelope.Result);
    }

    private static void AssertPageMetadata(
        PagedResult<DepartmentTree> page,
        int pageNumber,
        int pageSize,
        int totalCount,
        int pageCount)
    {
        Assert.Equal(pageNumber, page.PageNumber);
        Assert.Equal(pageSize, page.PageSize);
        Assert.Equal(totalCount, page.TotalCount);
        Assert.Equal(pageCount, page.PageCount);
    }
}
