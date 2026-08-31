# JobTrackerService

Modular monolith .NET con DDD, CQRS/MediatR, EF Core/PostgreSQL, outbox y Hangfire.

## Estructura

- `src/Jobs/Domain`: aggregate `Job`, `Address`, `JobPhoto`, invariantes y eventos.
- `src/Jobs/Application`: comandos, query, validadores y `Result<T>`.
- `src/Jobs/Infrastructure`: EF Core, repositorio parcial, Unit of Work y outbox.
- `src/Migrations`: proyecto independiente con migraciones de Entity Framework Core para el esquema `jobs`.
- `src/Api`: endpoints mínimos y dashboard de Hangfire.

## Ejecutar

1. Crear una base PostgreSQL `job_tracker`.
2. Restaurar la herramienta local: `dotnet tool restore`.
3. Aplicar las migraciones: `dotnet ef database update --project src/Jobs/Infrastructure/JobTrackerService.Jobs.Infrastructure.csproj --startup-project src/Api/JobTrackerService.Api.csproj`.
4. Configurar `ConnectionStrings:Jobs` en `src/Api/appsettings.json`.
5. Ejecutar `dotnet run --project src/Api`.

El outbox se escribe en la misma transacción que el agregado. Un evento de dominio coordina comportamiento dentro de Jobs; un evento de integración cruza módulos. El dispatcher reintenta mensajes pendientes, por lo que la entrega es al menos una vez. El consumidor de facturas debe imponer una restricción única sobre `JobId + CompletedAt`, usado como clave de idempotencia.
