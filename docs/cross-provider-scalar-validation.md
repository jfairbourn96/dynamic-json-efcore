# Cross-provider scalar validation

Feature 43 protects the provider-neutral scalar contract by running shared behavioral tests against
real SQL Server and PostgreSQL databases while retaining separate generated-SQL assertions for each
dialect.

## Test architecture

`ScalarProviderContractTests` owns behavior that must be identical across providers:

- populated, empty, and database-null `JsonObject` persistence;
- raw provider storage and fresh-context materialization;
- text, decimal, and date scalar execution; and
- missing property, JSON null, database null, and invalid conversion results.

Each provider supplies a small adapter that configures its EF Core provider, dynamic JSON
registration, physical JSON column type, raw-storage query, and database lifecycle. Both adapters
run against their provider's Testcontainers fixture through the shared `IScalarProviderFixture`
connection boundary.

Generated SQL remains provider-specific. SQL Server tests assert `JSON_VALUE` and `TRY_CONVERT`;
PostgreSQL tests assert `jsonb_path_query_first`, `pg_input_is_valid`, and typed casts. Both suites
verify captured paths and comparison values remain database parameters.

## Story coverage

| Story | Validation |
|---|---|
| #59 Shared provider contracts | One inherited persistence and scalar behavior suite runs for both providers through a common fixture boundary. |
| #60 PostgreSQL infrastructure | The PostgreSQL 18 Testcontainers fixture owns startup, connection configuration, shared database access, and disposal. |
| #61 PostgreSQL persistence | Shared and provider-specific tests cover `jsonb` storage, populated/empty/null values, raw JSON, and fresh-context materialization. |
| #62 PostgreSQL scalar execution | Real PostgreSQL tests execute text, decimal, and date queries plus missing, null, and invalid cases. |
| #63 SQL Server compatibility | The same behavioral contracts run against SQL Server 2022 while existing persistence, registration, path, and scalar regressions remain in place. |
| #64 Generated SQL | Separate dialect suites inspect readable provider SQL and captured path/comparison parameterization without requiring equivalent SQL text. |

Collection queries, benchmarks, execution-plan comparison, and SQL dialect equivalence remain out
of scope.

## Running the validation

The real-provider suite requires Docker:

```powershell
dotnet test Dynamic.Json.EfCore.IntegrationTests\Dynamic.Json.EfCore.IntegrationTests.csproj
```

Run only the shared contracts with:

```powershell
dotnet test Dynamic.Json.EfCore.IntegrationTests\Dynamic.Json.EfCore.IntegrationTests.csproj `
  --filter "FullyQualifiedName~ScalarProviderContractTests"
```
