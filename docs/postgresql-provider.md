# PostgreSQL Provider

`Dynamic.Json.EfCore.PostgreSql` adds PostgreSQL `jsonb` persistence and server-side scalar query translation to the provider-neutral Dynamic.Json.EfCore API.

## Installation

Install the core package and PostgreSQL provider. While the provider is in preview, include the prerelease flag:

```console
dotnet add package Dynamic.Json.EfCore --prerelease
dotnet add package Dynamic.Json.EfCore.PostgreSql --prerelease
```

## Registration

```csharp
services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(connectionString)
        .UseDynamicJsonPostgreSql());
```

If `UseDynamicJsonPostgreSql` is omitted, Dynamic.Json scalar methods fail translation rather than silently switching to client evaluation.

## `jsonb` mapping

```csharp
modelBuilder.Entity<Employee>()
    .Property(employee => employee.FieldValues)
    .HasColumnType("jsonb")
    .HasJsonConversion()
    .IsRequired(false);
```

## Scalar queries

Build paths with `DynamicJsonPath`; captured paths and comparison values are parameterized.

```csharp
var departmentPath = DynamicJsonPath.FromProperty("department");
var salaryPath = DynamicJsonPath.FromProperty("salary");
var hiredOnPath = DynamicJsonPath.FromProperty("hiredOn");
var cutoffDate = new DateOnly(2025, 1, 1);

var employees = await context.Employees
    .Where(employee =>
        DynamicJsonFunctions.Value(employee.FieldValues, departmentPath) == "Engineering"
        && DynamicJsonFunctions.ValueDecimal(employee.FieldValues, salaryPath) >= 100_000m
        && DynamicJsonFunctions.ValueDate(employee.FieldValues, hiredOnPath) >= cutoffDate)
    .ToListAsync();
```

### Generated SQL

These fragments illustrate the translation shape. EF Core may change aliases and parameter names:

```sql
jsonb_path_query_first(e."FieldValues", @path) #>> '{}'
```

```sql
CASE
    WHEN pg_input_is_valid(jsonb_path_query_first(e."FieldValues", @path) #>> '{}', 'numeric')
    THEN CAST(jsonb_path_query_first(e."FieldValues", @path) #>> '{}' AS numeric)
END
```

`ValueDate` uses the same guarded shape with the `timestamp without time zone` PostgreSQL type.

## Compatibility

| Package or runtime | Supported version |
| --- | --- |
| Dynamic.Json.EfCore.PostgreSql | `0.2.1-preview.1` |
| .NET | 10 |
| EF Core relational | `>= 10.0.9` and `< 11.0.0` |
| Npgsql EF Core provider | `>= 10.0.3` and `< 11.0.0` |
| PostgreSQL server | 16 or later; tested against PostgreSQL 18 |

Minor dependency updates inside these ranges are supported by the package metadata. New EF Core or Npgsql major versions require an explicit compatibility review and package update. This matrix does not imply support for older versions.

## Cross-provider behavior

| Concern | PostgreSQL | SQL Server |
| --- | --- | --- |
| JSON storage | Native `jsonb` | JSON text in `nvarchar(max)` |
| Scalar extraction | `jsonb_path_query_first` | `JSON_VALUE` |
| Missing path | SQL `NULL` | SQL `NULL` |
| JSON `null` | SQL `NULL` | SQL `NULL` |
| Invalid decimal/date conversion | SQL `NULL` | SQL `NULL` |
| Path API | `DynamicJsonPath` | `DynamicJsonPath` |

The portable contract covers paths, null handling, and safe scalar conversions. Storage representation and generated SQL remain provider-specific.

## Preview limitations

This preview does not make stable 1.0 compatibility guarantees. Collection-query documentation, performance tuning guidance, and deployment guidance are outside its scope.
