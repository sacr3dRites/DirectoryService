# DirectoryService

## Database seeding

The seeder expects the database schema to exist and does not apply migrations. It inserts 100 locations,
departments, positions, department-location links, and department-position links in one transaction.

```powershell
cd DirectoryService
dotnet run --project src/DirectoryService.Seeder/DirectoryService.Seeder.csproj
```

The command uses the connection string from `DirectoryService.Presentation/appsettings.json`.
It can be overridden with the `ConnectionStrings__DirectoryServiceDb` environment variable.

Existing application data is preserved, so the resulting tables may contain more than 100 rows. Re-running
the command after a complete seed is safe and does not add duplicates. If a partial or modified seed data set
is detected, the command stops without changing it.
