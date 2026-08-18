namespace DirectoryService.IntegrationTests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class DirectoryIntegrationTestCollection : ICollectionFixture<DirectoryTestWebFactory>
{
    public const string Name = "Directory Service integration tests";
}
