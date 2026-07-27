# Security review: Feature 42 PostgreSQL scalar translation

## Scope and status

This point-in-time review covers PostgreSQL scalar query translation through Story 58:

- `Value`, `ValueDecimal`, and `ValueDate` translation;
- portable constant and captured JSON paths;
- captured comparison values;
- missing, JSON-null, database-null, and invalid conversion behavior;
- PostgreSQL provider registration and dependency isolation; and
- relevant direct and transitive NuGet dependencies.

Review date: July 27, 2026.

This is an engineering assessment of the reviewed implementation, not a guarantee that the package,
PostgreSQL, EF Core, Npgsql, or their dependencies will remain free of vulnerabilities.

## Threat model

JSON paths and comparison values may originate from runtime metadata, tenant configuration, HTTP
requests, or other untrusted sources. Relevant risks include SQL or JSON-path injection, conversion
errors that abort a query, accidental literal embedding, provider-specific syntax escaping the
portable contract, and PostgreSQL behavior leaking into provider-neutral core.

Field authorization remains an application responsibility. A safe and valid path does not establish
that a caller may search the represented field.

## SQL construction and parameterization

The PostgreSQL translator builds expressions through Npgsql and EF Core SQL expression factories.
It does not concatenate paths or comparison values into raw SQL.

Constant paths are normalized through `DynamicJsonPath` before becoming typed `jsonpath`
expressions. Captured paths remain PostgreSQL `jsonpath` parameters, and captured string, decimal,
and date comparison values remain ordinary EF Core parameters. Generated-SQL regression coverage
verifies parameter use for all three scalar marker functions.

Provider-owned constants are limited to fixed PostgreSQL function names and store types such as
`jsonpath`, `text`, `numeric`, and `date`. No user-controlled value is used as a SQL identifier,
alias, function name, store type, or SQL fragment.

## Null and conversion safety

Scalar extraction uses `jsonb_path_query_first` and text traversal. Missing properties, JSON null,
and database-null JSON documents produce SQL `NULL`.

Decimal and date translations guard casts with `pg_input_is_valid`. Invalid input therefore
produces SQL `NULL` instead of raising a cast error. Accepted values follow PostgreSQL's native
`numeric` and `date` input rules.

The guard requires PostgreSQL 16 or later. Current real-provider coverage runs against PostgreSQL
18; older server versions are not supported by this translation.

## Portable path boundary

Constant paths are restricted to the provider-neutral scalar property subset. Array indexes,
wildcards, filters, recursive descent, relative paths, and provider modes remain unsupported.
Captured paths should be constructed with `DynamicJsonPath` before EF Core parameterizes them.

The existing compatibility constraint still applies: a translator cannot inspect the runtime value
inside an EF parameter expression. Applications must not pass arbitrary raw runtime strings when
they intend to enforce the portable path grammar.

## Provider isolation

PostgreSQL dependencies, type mappings, SQL functions, service registration, and translation remain
inside `Dynamic.Json.EfCore.PostgreSql`. Provider-neutral core has no Npgsql dependency, and the
existing SQL Server translator and registration behavior remain unchanged.

## Validation performed

The reviewed feature passed:

- 108 unit tests;
- 39 Docker/Testcontainers-backed provider integration tests across PostgreSQL and SQL Server;
- generated-SQL inspection for captured path and comparison parameters;
- real PostgreSQL missing, null, invalid, and valid scalar behavior; and
- `git diff --check`.

The NuGet vulnerability audit reported no known vulnerable packages from the configured sources on
July 27, 2026. This result is time-sensitive and must be repeated by CI and before release.

## Residual risks and recommendations

- Applications must authorize searchable fields before constructing queries.
- Runtime property names should be passed through `DynamicJsonPath` builders.
- Input size and query complexity limits remain application responsibilities.
- PostgreSQL 16 is the minimum server version implied by `pg_input_is_valid`; only PostgreSQL 18 is
  currently exercised in integration tests.
- Changes to PostgreSQL conversion functions, path grammar, raw SQL, custom SQL generators, or
  provider major versions require a new review.
- Collection traversal remains out of scope and requires its own security analysis.
