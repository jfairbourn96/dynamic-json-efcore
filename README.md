# Dynamic.Json.EfCore

Dynamic.Json.EfCore is a package-oriented .NET repository for mapping, tracking, validating, and querying dynamic JSON values with EF Core.

The package set is designed for applications that store user-defined or schema-flexible fields as JSON while still wanting strongly modeled EF Core configuration, change tracking, query validation, and provider-specific SQL translation.

## Highlights

- Provider-neutral JSON mapping/query primitives for EF Core.
- Provider-neutral dynamic search filter models and parser.
- ASP.NET Core adapters for query-string based dynamic search.
- SQL Server-specific EF Core translation package for JSON query functions.
- Unit and integration test projects separated by test boundary.
- Security-conscious query construction using EF expression translation rather than raw SQL string building.

## Repository Layout

```text
Dynamic.Json.Search/                  Provider-neutral dynamic search filter models and parser
Dynamic.Json.EfCore/                  Provider-neutral JSON mapping, tracking, and query markers
Dynamic.Json.AspNetCore/              ASP.NET Core dynamic search query adapters
Dynamic.Json.EfCore.SqlServer/        SQL Server EF Core JSON query translations
Dynamic.Json.EfCore.UnitTests/        Unit tests for the package set
Dynamic.Json.EfCore.IntegrationTests/ Integration test shell for future Docker-backed provider tests
docs/                                 Package documentation and test coverage notes
TODO.md                               Follow-up work and publishing checklist
```

## Package Boundaries

The reusable JSON functionality is split into package-sized projects:

| Project | Purpose |
|---|---|
| `Dynamic.Json.Search` | Provider-neutral dynamic search field/filter models, parser, and parse result/error contracts. |
| `Dynamic.Json.EfCore` | Provider-neutral EF Core primitives for JSON conversion, value comparison, and EF query marker functions. |
| `Dynamic.Json.AspNetCore` | Adapts ASP.NET Core query-string collections to the provider-neutral search parser and registers parser services. |
| `Dynamic.Json.EfCore.SqlServer` | Translates provider-neutral JSON query functions into SQL Server functions such as `JSON_VALUE` and `TRY_CONVERT`. |

This split keeps the base search model free of ASP.NET Core, EF Core, and SQL Server concerns. Application layers can depend on `Dynamic.Json.Search`, API layers can opt into `Dynamic.Json.AspNetCore`, and infrastructure layers can choose the EF/provider package they need.

## Design Decisions

`Dynamic.Json.Search` owns the search language. Search fields, operators, parsed filters, parse results, and parse errors are not EF-specific concepts, so they live in a package that can be used by application services, workers, tests, or non-HTTP entry points.

`Dynamic.Json.AspNetCore` is an adapter package. It can translate `IQueryCollection` into the provider-neutral parser input and register ASP.NET Core services, but it should not become the place where business validation or database querying lives.

`Dynamic.Json.EfCore` owns EF Core primitives that are not database-provider-specific: JSON conversion, value comparison, and marker functions used in LINQ expressions.

`Dynamic.Json.EfCore.SqlServer` owns SQL Server translation. SQL Server details such as `JSON_VALUE`, `TRY_CONVERT`, and store type fragments stay out of the provider-neutral EF package. Future providers can be added as sibling packages instead of changing application code that only knows about search criteria.

A typical clean architecture dependency chain looks like this:

```text
Application -> Dynamic.Json.Search
API         -> Dynamic.Json.AspNetCore
Data        -> Dynamic.Json.EfCore.SqlServer
```

That shape lets an application parse and validate search criteria without taking a dependency on ASP.NET Core or EF Core. The repository/infrastructure layer receives validated filters and decides how to translate them for the selected database provider.

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

The provider-neutral parser converts key/value pairs into typed dynamic filters. For example:

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

ASP.NET Core applications can use `Dynamic.Json.AspNetCore` to adapt `IQueryCollection` into the provider-neutral parser. Non-HTTP applications can pass dictionaries or other simple key/value inputs directly to `Dynamic.Json.Search`.

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

Coverage:

```powershell
dotnet test Dynamic.Json.EfCore.UnitTests\Dynamic.Json.EfCore.UnitTests.csproj --settings coverlet.runsettings --results-directory artifacts\coverage\raw --collect "XPlat Code Coverage"
```

CI generates an HTML/Cobertura coverage report from the unit test suite, publishes the Markdown summary to the GitHub Actions job summary, and uploads the full report as a `coverage-report` artifact. The integration project currently contains a placeholder, so it is not included in the default coverage report until it has real provider tests.

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
