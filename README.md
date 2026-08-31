# JobTrackerService

A .NET modular monolith built with DDD, CQRS/MediatR, EF Core, PostgreSQL, and an outbox pattern.

## Project structure

- `src/Jobs/Domain`: aggregate `Job`, `Address`, `JobPhoto`, invariants, and domain events.
- `src/Jobs/Application`: commands, queries, validators, and `Result<T>` wrapper.
- `src/Jobs/Infrastructure`: EF Core configuration, repository, Unit of Work, and outbox implementation.
- `src/Migrations`: EF Core migrations project.
- `src/Api`: HTTP endpoints and app startup configuration.

## Prerequisites

- .NET SDK 9+
- PostgreSQL instance
- Access to the project folder and a terminal

## Step-by-step setup

1. Create a PostgreSQL database named `job_tracker`.
2. Open a terminal in the backend root:
   ```bash
   cd /run/media/juan/Archivos/Proyectos/Desarrollos/Juan/Pruebas/JobTrackerService
   ```
3. Restore .NET tooling if needed:
   ```bash
   dotnet tool restore
   ```
4. Configure the database connection string in `src/Api/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "Jobs": "Host=localhost;Port=5432;Database=job_tracker;Username=postgres;Password=your_password"
     }
   }
   ```
5. Apply the migrations:
   ```bash
   dotnet ef database update \
     --project src/Jobs/Infrastructure/JobTrackerService.Jobs.Infrastructure.csproj \
     --startup-project src/Api/JobTrackerService.Api.csproj
   ```
6. Start the API:
   ```bash
   dotnet run --project src/Api/JobTrackerService.Api.csproj
   ```
7. The app should start on the configured localhost port, usually `http://localhost:5028`.

## Run tests

Run the full backend test suite:

```bash
cd /run/media/juan/Archivos/Proyectos/Desarrollos/Juan/Pruebas/JobTrackerService
dotnet test JobTrackerService.sln --nologo
```

Run only the unit test project:

```bash
cd /run/media/juan/Archivos/Proyectos/Desarrollos/Juan/Pruebas/JobTrackerService
dotnet test tests/JobTrackerService.Tests/JobTrackerService.Tests.csproj --nologo
```

## Notes

- The outbox is written in the same transaction as the aggregate state.
- Domain events coordinate behavior inside the Jobs bounded context.
- Integration events can be dispatched outward once the transactional work succeeds.
- Delivery is at least once, so consumers should use idempotent logic when needed.
