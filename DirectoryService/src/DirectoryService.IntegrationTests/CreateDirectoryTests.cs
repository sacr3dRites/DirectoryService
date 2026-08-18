using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Infrastructure;
using DirectoryService.Shared.CustomErrors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests;

public class CreateDirectoryTests : DirectoryTestsBase
{
    public CreateDirectoryTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateLocationWithValidDataShouldSucceed()
    {
        // arrange
        var command =
            new CreateLocationCommand(new CreateLocationRequest("Main Office", "BruhStreet", "Europe/Moscow"));

        // act
        var cancellationToken = CancellationToken.None;
        var result = await ExecuteHandler(command, cancellationToken);

        // assert
        await using var assertScope = Services.CreateAsyncScope();
        var dbContext = assertScope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        var location = await dbContext.Locations.FirstAsync(l => l.Id == result.Value, cancellationToken);

        Assert.NotNull(location);
        Assert.Empty(result.IsFailure ? result.Error : []);
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task CreateDepartmentWithValidDataShouldSucceed()
    {
        // arrange
        var locationId = await CreateLocation();

        var command =
            new CreateDepartmentCommand(new CreateDepartmentRequest("Main Dep", "maindep", [locationId]));

        // act
        var cancellationToken = CancellationToken.None;
        var result = await ExecuteHandler(command, cancellationToken);

        // assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments.FirstAsync(d => d.Id == result.Value, cancellationToken);

            Assert.NotNull(department);
            Assert.Empty(result.IsFailure ? result.Error : []);
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
        });
    }

    [Fact]
    public void Test3()
    {
    }

    private async Task<Guid> CreateLocation()
    {
        return await ExecuteInDb(async dbContext =>
        {
            var location = Location.Create(
                CorrectLocationName.Create("testLocation").Value,
                LocationAddress.Create("testAddress").Value,
                Timezone.Create("Europe/Moscow").Value);

            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync();
            return location.Id;
        });
    }

    private async Task<Result<Guid, Errors>> ExecuteHandler<TCommand>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        await using var scope = Services.CreateAsyncScope();

        var handler = scope.ServiceProvider
            .GetRequiredService<
                ICommandHandler<Result<Guid, Errors>, TCommand>>();

        return await handler.Handle(command, cancellationToken);
    }
}