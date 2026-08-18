# Directory Service integration tests

Prerequisites: .NET 9 SDK and a running Docker-compatible daemon (for example, Docker Desktop).

From the `DirectoryService` directory, run only the Directory Service integration suite:

```powershell
.\scripts\run-integration-tests.ps1
```

For a repeat run without rebuilding:

```powershell
.\scripts\run-integration-tests.ps1 -NoBuild
```

The equivalent cross-platform command is:

```text
dotnet test src/DirectoryService.IntegrationTests/DirectoryService.IntegrationTests.csproj
```

This command targets the standalone integration-test project, so it does not run unit tests or frontend checks.

The suite starts a real PostgreSQL container, points both EF Core and Dapper at it, applies EF Core migrations, and exercises the real ASP.NET Core pipeline with `WebApplicationFactory<Program>`. All tests share one non-parallel xUnit collection. `Respawn` clears application tables before and after every scenario while preserving `__EFMigrationsHistory`, so tests do not depend on previous data or execution order.

`ManualReferenceTests.cs` contains the three manually authored reference scenarios. Additional endpoint scenarios are kept under `Generated/` and follow the same pattern: assert the HTTP status and Envelope body, then verify PostgreSQL state for write operations.
