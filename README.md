# Dynamic.Json.EfCore

Dynamic.Json.EfCore is a package-oriented .NET repository for mapping, tracking, validating, and querying dynamic JSON values with EF Core.

The package set is designed for applications that store user-defined or schema-flexible fields as JSON while still wanting strongly modeled EF Core configuration, change tracking, query validation, and provider-specific SQL translation.

## Highlights

- Provider-neutral JSON mapping/query primitives for EF Core.
- SQL Server-specific EF Core translation package for JSON query functions.
- ASP.NET Core query-string parsing package for dynamic search filters.
- Unit and integration test projects separated by test boundary.
- Security-conscious query construction using EF expression translation rather than raw SQL string building.

## Repository Layout

```text
Dynamic.Json.EfCore/                  Provider-neutral JSON mapping, tracking, and query markers
Dynamic.Json.EfCore.AspNetCore/       ASP.NET Core dynamic search query parsing
Dynamic.Json.EfCore.SqlServer/        SQL Server EF Core JSON query translations
Dynamic.Json.EfCore.UnitTests/        Unit tests for the package set
Dynamic.Json.EfCore.IntegrationTests/ Integration test shell for future Docker-backed provider tests
docs/                                 Package documentation and test coverage notes
TODO.md                               Follow-up work and publishing checklist
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

## Building

Build the package solution:

```powershell
dotnet build Dynamic.Json.EfCore.slnx
```

## Tests

Unit tests:

```powershell
dotnet test Dynamic.Json.EfCore.UnitTests\Dynamic.Json.EfCore.UnitTests.csproj
```

Integration test shell:

```powershell
dotnet test Dynamic.Json.EfCore.IntegrationTests\Dynamic.Json.EfCore.IntegrationTests.csproj
```

The current integration project contains a placeholder. Docker/Testcontainers-backed SQL Server tests are tracked in `TODO.md`.

Coverage notes for the package set live in `docs/test-coverage.md`.

## Security Notes

- Dynamic JSON search uses EF Core expression translation rather than raw SQL construction.
- Dynamic field names are validated before they are converted into JSON paths.
- LIKE patterns escape wildcard characters before filtering.
- Package vulnerability checks are part of the review workflow:

```powershell
dotnet list Dynamic.Json.EfCore.slnx package --vulnerable --include-transitive
```

## Roadmap

Near-term follow-up work is tracked in `TODO.md`, including:

- Docker-backed SQL Server integration tests.
- Provider translation tests outside the unit test project.
- Future PostgreSQL support.
- Future Newtonsoft/JObject support.
- Swagger/OpenAPI documentation after selecting a package version without known vulnerabilities.
