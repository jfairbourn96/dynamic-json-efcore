# Dynamic HR in EF Core

Dynamic HR in EF Core is a proof-of-concept HR records system built with .NET 10, EF Core, SQL Server, and React. The project explores a common product problem: how to support user-defined fields, dynamic forms, and dynamic search without creating a new database column for every custom field.

The backend stores employee-specific custom field values as JSON and uses reusable `Dynamic.Json.EfCore.*` packages to map, track, validate, and query those JSON values through EF Core.

## Highlights

- Dynamic employee type definitions with custom field schemas.
- Employee records with structured JSON field values.
- Runtime-generated forms and search filters in React.
- Provider-neutral JSON mapping/query primitives for EF Core.
- SQL Server-specific EF Core translation package for JSON query functions.
- ASP.NET Core query-string parsing package for dynamic search filters.
- Unit and integration test projects separated by test boundary.
- Security-conscious query construction using EF expression translation rather than raw SQL string building.

## Repository Layout

```text
backend/DynamicEmployeeApi/
  Dynamic.Json.EfCore/                 Provider-neutral JSON mapping, tracking, and query markers
  Dynamic.Json.EfCore.AspNetCore/      ASP.NET Core dynamic search query parsing
  Dynamic.Json.EfCore.SqlServer/       SQL Server EF Core JSON query translations
  Dynamic.Json.EfCore.UnitTests/       Unit tests for the Dynamic.Json.EfCore package set
  Dynamic.Json.EfCore.IntegrationTests/Integration test shell for future Docker-backed provider tests
  Dynamic.Employees.Core/              Domain models and enums
  Dynamic.Employees.Data/              EF Core DbContext and data configuration
  EmployeeApi/                         ASP.NET Core API

frontend/
  src/                                 React UI for employee types, forms, and search

docs/
  test-coverage.md                     Current test coverage notes for Dynamic.Json.EfCore packages
```

## Dynamic.Json.EfCore Packages

The reusable JSON functionality is split into package-sized projects:

| Project | Purpose |
|---|---|
| `Dynamic.Json.EfCore` | Provider-neutral primitives for JSON conversion, value comparison, search filter models, and EF query marker functions. |
| `Dynamic.Json.EfCore.AspNetCore` | Parses ASP.NET Core query-string parameters into validated `DynamicSearchFilter` objects. |
| `Dynamic.Json.EfCore.SqlServer` | Translates provider-neutral JSON query functions into SQL Server functions such as `JSON_VALUE` and `TRY_CONVERT`. |

This split keeps the base package free of SQL Server and ASP.NET Core concerns, while leaving room for future provider packages such as PostgreSQL and future JSON object models such as Newtonsoft `JObject`.

## JSON Mapping and Change Tracking

`HasJsonConversion()` configures a `JsonObject` property to be stored as serialized JSON and tracked deeply by EF Core:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>(entity =>
    {
        entity.Property(e => e.FieldValues).HasJsonConversion();
    });
}
```

By default, `HasJsonConversion()` uses `JsonObjectComparisonMode.Semantic`:

```csharp
entity.Property(e => e.FieldValues)
    .HasJsonConversion(JsonObjectComparisonMode.Semantic);
```

Semantic comparison treats JSON objects as structured data rather than serialized text:

- Object property order does not affect equality.
- Nested objects are compared recursively.
- Arrays are compared in order, so array ordering remains significant.
- `null` equals `null`, but `null` does not equal a populated JSON object.
- `null` hashes to `0`.

These objects are considered equal because they contain the same properties and values:

```json
{ "name": "Elsa", "role": "Queen" }
```

```json
{ "role": "Queen", "name": "Elsa" }
```

Arrays remain order-sensitive, so these arrays are not equal:

```json
["Sven", "Olaf"]
```

```json
["Olaf", "Sven"]
```

For applications that prefer faster, property-order-sensitive comparison, use `JsonObjectComparisonMode.Serialized`:

```csharp
entity.Property(e => e.FieldValues)
    .HasJsonConversion(JsonObjectComparisonMode.Serialized);
```

Use semantic comparison when JSON object property order should not matter. Use serialized comparison when raw comparison speed is more important and callers are comfortable with property-order-sensitive change detection.

## Dynamic Search

The ASP.NET Core parser converts query-string keys into typed dynamic filters. For example:

```text
favoriteSongName_contains=Go
numberOfSongs_gte=7
hasIcePowers=true
```

The parser validates:

- supported field names
- supported operators for each field type
- number, date, boolean, and select-option values
- ignored framework/application query parameters such as paging keys

Errors are returned as structured parse errors with stable error codes, allowing API consumers to format or localize messages without relying on exception text.

## SQL Server Translation

`Dynamic.Json.EfCore.SqlServer` translates provider-neutral marker functions into SQL Server expressions:

```csharp
DynamicJsonFunctions.Value(employee.FieldValues, "$.favoriteSongName")
DynamicJsonFunctions.ValueDecimal(employee.FieldValues, "$.numberOfSongs")
DynamicJsonFunctions.ValueDate(employee.FieldValues, "$.coronationDate")
```

The SQL Server package plugs into EF Core through:

```csharp
options.UseSqlServer(connectionString)
    .UseDynamicJsonSqlServer();
```

The translator uses EF Core SQL expression APIs instead of raw SQL string concatenation. Store type fragments used by `TRY_CONVERT` are fixed internally, and user values are kept in EF expression translation.

## Running the Project

Backend:

```powershell
dotnet build backend\DynamicEmployeeApi\DynamicEmployeeApi.sln
dotnet run --project backend\DynamicEmployeeApi\EmployeeApi\EmployeeApi.csproj
```

Frontend:

```powershell
cd frontend
npm install
npm run dev
```

The API is configured for SQL Server LocalDB by default in `EmployeeApi/appsettings.json`. Update the `DefaultConnection` connection string as needed for your environment.

## Tests

Unit tests:

```powershell
dotnet test backend\DynamicEmployeeApi\Dynamic.Json.EfCore.UnitTests\Dynamic.Json.EfCore.UnitTests.csproj
```

Integration test shell:

```powershell
dotnet test backend\DynamicEmployeeApi\Dynamic.Json.EfCore.IntegrationTests\Dynamic.Json.EfCore.IntegrationTests.csproj
```

The current integration project contains a placeholder. Docker/Testcontainers-backed SQL Server tests are tracked in `TODO.md`.

Coverage notes for the package set live in `docs/test-coverage.md`.

## Security Notes

- Dynamic JSON search uses EF Core expression translation rather than raw SQL construction.
- Dynamic field names are validated before they are converted into JSON paths.
- LIKE patterns escape wildcard characters before filtering.
- Package vulnerability checks are part of the review workflow:

```powershell
dotnet list backend\DynamicEmployeeApi\DynamicEmployeeApi.sln package --vulnerable --include-transitive
```

## Roadmap

Near-term follow-up work is tracked in `TODO.md`, including:

- Docker-backed SQL Server integration tests.
- Provider translation tests outside the unit test project.
- Future PostgreSQL support.
- Future Newtonsoft/JObject support.
- Swagger/OpenAPI documentation after selecting a package version without known vulnerabilities.
