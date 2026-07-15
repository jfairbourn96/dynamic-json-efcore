# Dynamic.Json.EfCore Test Coverage

This checklist tracks meaningful test coverage for the `Dynamic.Json.EfCore.*` package set. Update it when package behavior changes or when new tests close a gap.

## Current Test Status

Last verified on July 15, 2026, with:

```text
dotnet test Dynamic.Json.EfCore.UnitTests\Dynamic.Json.EfCore.UnitTests.csproj --no-restore
Passed: 86, Failed: 0, Skipped: 0
```

Coverage collection:

```text
dotnet test Dynamic.Json.EfCore.UnitTests\Dynamic.Json.EfCore.UnitTests.csproj --settings coverlet.runsettings --results-directory artifacts\coverage\raw --collect "XPlat Code Coverage"
```

Current unit-test product-code baseline:

```text
Line coverage: 96.34%
Branch coverage: 90.41%
```

| Package | Line coverage | Branch coverage |
|---|---:|---:|
| `Dynamic.Json.Metadata` | 100.00% | 100.00% |
| `Dynamic.Json.Search` | 99.41% | 89.65% |
| `Dynamic.Json.EfCore` | 87.20% | 82.60% |
| `Dynamic.Json.AspNetCore` | 84.21% | 100.00% |

The default coverage report excludes test assemblies and the SQL Server integration project. Integration tests run in a separate CI job because they require Docker/Testcontainers and a real SQL Server container.

## Coverage Matrix

| Project | Area | Status | Notes |
|---|---|---|---|
| `Dynamic.Json.Metadata` | Runtime metadata contracts | Covered | Scalar and JsonArray definitions serialize and deserialize without constructor-thrown validation. |
| `Dynamic.Json.Metadata` | Structured metadata validation | Covered | Valid metadata, every error code, aggregate errors, stable paths, null inputs, duplicate fields/options, and invalid array/select configurations. |
| `Dynamic.Json.Metadata` | Fail-fast validation | Covered | `ValidateAndThrow()` returns valid metadata and exposes the complete result through `DynamicMetadataValidationException` for invalid metadata. |
| `Dynamic.Json.EfCore` | Semantic JSON comparison | Covered | Equality, property-order behavior, nested objects, arrays, nulls, hashing, snapshots. |
| `Dynamic.Json.EfCore` | Serialized JSON comparison | Covered | Serialized equality, property-order sensitivity, nulls, hashing, snapshots. |
| `Dynamic.Json.EfCore` | `HasJsonConversion()` | Covered | Default semantic comparer and explicit serialized comparer are verified through EF model metadata. SQL Server persistence round trips are covered by integration tests. |
| `Dynamic.Json.EfCore` | Query marker functions | Covered | Direct invocation throws for `Value`, `ValueDecimal`, and `ValueDate`. |
| `Dynamic.Json.Search` | Search records/enums | Mostly covered | `DynamicSearchField` default options are covered. Other records/enums are simple contracts and covered indirectly through parser tests. |
| `Dynamic.Json.AspNetCore` | Dynamic search query parsing | Covered | Happy paths, select fields, invalid operators, invalid values, unknown fields, ignored parameters, mixed valid/invalid input, multiple errors. |
| `Dynamic.Json.AspNetCore` | Parser service registration | Covered | `AddDynamicJsonAspNetCore()` resolves `IDynamicSearchQueryParser`. |
| `Dynamic.Json.EfCore.SqlServer` | Provider registration | Integration covered | SQL Server integration contexts call `UseDynamicJsonSqlServer()` against the real provider. |
| `Dynamic.Json.EfCore.SqlServer` | SQL translation | Integration covered | `ToQueryString()` verifies `JSON_VALUE`, `TRY_CONVERT(decimal(18, 4), ...)`, and `TRY_CONVERT(date, ...)` output. |
| `Dynamic.Json.EfCore.SqlServer` | Runtime SQL behavior | Integration covered | Docker/Testcontainers-backed SQL Server tests cover persistence, string JSON lookup, numeric conversion/search, and date conversion/search. |

## Future Coverage Triggers

Add or update tests when any of these change:

- New `DynamicJsonFunctions` methods are added.
- Runtime metadata field types or validation error codes are added.
- Search operators or field types are added.
- Parser error codes or parser options are added.
- JSON comparison semantics change.
- Provider-specific SQL translation changes.
- A new database provider package is introduced.
- Newtonsoft/JObject support is introduced.
