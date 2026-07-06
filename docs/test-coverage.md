# Dynamic.Json.EfCore Test Coverage

This checklist tracks meaningful test coverage for the `Dynamic.Json.EfCore.*` package set. Update it when package behavior changes or when new tests close a gap.

## Current Test Status

Last verified with:

```text
dotnet test Dynamic.Json.EfCore.UnitTests\Dynamic.Json.EfCore.UnitTests.csproj --no-restore
Passed: 66, Failed: 0, Skipped: 0
```

## Coverage Matrix

| Project | Area | Status | Notes |
|---|---|---|---|
| `Dynamic.Json.EfCore` | Semantic JSON comparison | Covered | Equality, property-order behavior, nested objects, arrays, nulls, hashing, snapshots. |
| `Dynamic.Json.EfCore` | Serialized JSON comparison | Covered | Serialized equality, property-order sensitivity, nulls, hashing, snapshots. |
| `Dynamic.Json.EfCore` | `HasJsonConversion()` | Unit covered | Default semantic comparer and explicit serialized comparer are verified through EF model metadata. Persistence round trips belong in integration tests. |
| `Dynamic.Json.EfCore` | Query marker functions | Covered | Direct invocation throws for `Value`, `ValueDecimal`, and `ValueDate`. |
| `Dynamic.Json.EfCore` | Search records/enums | Mostly covered | `DynamicSearchField` default options are covered. Other records/enums are simple contracts and covered indirectly through parser tests. |
| `Dynamic.Json.EfCore.AspNetCore` | Dynamic search query parsing | Covered | Happy paths, select fields, invalid operators, invalid values, unknown fields, ignored parameters, mixed valid/invalid input, multiple errors. |
| `Dynamic.Json.EfCore.AspNetCore` | Parser service registration | Covered | `AddDynamicJsonEfCoreAspNetCore()` resolves `IDynamicSearchQueryParser`. |
| `Dynamic.Json.EfCore.SqlServer` | Provider registration | Planned integration/provider coverage | Track in `TODO.md`; requires SQL Server provider configuration. |
| `Dynamic.Json.EfCore.SqlServer` | SQL translation | Planned integration/provider coverage | Track in `TODO.md`; `ToQueryString()` tests should live outside the unit test project. |
| `Dynamic.Json.EfCore.SqlServer` | Runtime SQL behavior | Planned integration coverage | `Dynamic.Json.EfCore.IntegrationTests` exists with a placeholder; Docker/Testcontainers implementation is tracked in `TODO.md`. |

## Future Coverage Triggers

Add or update tests when any of these change:

- New `DynamicJsonFunctions` methods are added.
- Search operators or field types are added.
- Parser error codes or parser options are added.
- JSON comparison semantics change.
- Provider-specific SQL translation changes.
- A new database provider package is introduced.
- Newtonsoft/JObject support is introduced.
