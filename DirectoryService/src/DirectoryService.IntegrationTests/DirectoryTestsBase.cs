using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Infrastructure;
using DirectoryService.Shared.CustomErrors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests;

public abstract class DirectoryTestsBase : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly DirectoryTestWebFactory _factory;

    protected DirectoryTestsBase(DirectoryTestWebFactory factory)
    {
        _factory = factory;
        Client = factory.CreateClient();
        Services = factory.Services;
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _factory.ResetDatabaseAsync();
    }

    protected HttpClient Client { get; }

    protected IServiceProvider Services { get; }

    protected static Guid AssertValidId(Guid id)
    {
        Assert.NotEqual(Guid.Empty, id);
        return id;
    }

    protected async Task<ApiEnvelope<T>> AssertSuccessEnvelopeAsync<T>(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var envelope = await ReadEnvelopeAsync<T>(response);

        Assert.Null(envelope.Errors);
        Assert.NotEqual(default, envelope.TimeGenerated);
        return envelope;
    }

    protected async Task<ApiEnvelope<object>> AssertErrorEnvelopeAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        ErrorType expectedType,
        string expectedCode)
    {
        Assert.Equal(expectedStatus, response.StatusCode);

        var envelope = await ReadEnvelopeAsync<object>(response);

        Assert.Null(envelope.Result);
        Assert.NotEqual(default, envelope.TimeGenerated);
        Assert.NotNull(envelope.Errors);
        var error = Assert.Single(envelope.Errors);
        Assert.Equal(expectedType, error.Type);
        Assert.Equal(expectedCode, error.Code);
        Assert.False(string.IsNullOrWhiteSpace(error.Message));
        return envelope;
    }

    protected async Task<Guid> CreateLocationThroughApiAsync(
        string name = "Setup Office",
        string address = "Setup Street 1")
    {
        using var response = await Client.PostAsJsonAsync(
            "/api/locations",
            new CreateLocationRequest(name, address, "UTC"));

        var envelope = await AssertSuccessEnvelopeAsync<Guid>(response);
        return AssertValidId(envelope.Result);
    }

    protected async Task<Guid> CreateDepartmentThroughApiAsync(
        Guid[] locationIds,
        string name = "Setup Department",
        string identifier = "setup-department",
        Guid? parentId = null)
    {
        using var response = await Client.PostAsJsonAsync(
            "/api/departments",
            new CreateDepartmentRequest(name, identifier, locationIds, parentId));

        var envelope = await AssertSuccessEnvelopeAsync<Guid>(response);
        return AssertValidId(envelope.Result);
    }

    protected async Task<T> ExecuteInDbAsync<T>(Func<DirectoryServiceDbContext, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();
        return await action(dbContext);
    }

    protected async Task ExecuteInDbAsync(Func<DirectoryServiceDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();
        await action(dbContext);
    }

    protected Task<int> CountLocationsAsync() =>
        ExecuteInDbAsync(dbContext => dbContext.Locations.CountAsync());

    protected Task<int> CountDepartmentsAsync() =>
        ExecuteInDbAsync(dbContext => dbContext.Departments.CountAsync());

    protected Task<int> CountPositionsAsync() =>
        ExecuteInDbAsync(dbContext => dbContext.Positions.CountAsync());

    private static async Task<ApiEnvelope<T>> ReadEnvelopeAsync<T>(HttpResponseMessage response)
    {
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<T>>(JsonOptions);
        Assert.NotNull(envelope);
        return envelope;
    }
}

public sealed record ApiEnvelope<T>(T? Result, ApiError[]? Errors, DateTime TimeGenerated);

public sealed record ApiError(string Code, string Message, ErrorType Type, string? InvalidField);
