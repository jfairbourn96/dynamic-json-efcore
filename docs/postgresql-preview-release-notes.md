# PostgreSQL Preview Release Notes

## 0.3.0-preview.1

This preview introduces the `Dynamic.Json.EfCore.PostgreSql` package for PostgreSQL 16+, Npgsql, and EF Core 10.

### Included

- `jsonb` persistence using the shared Dynamic.Json conversion.
- `UseDynamicJsonPostgreSql` provider registration.
- Server-side `Value`, `ValueDecimal`, and `ValueDate` translation.
- Provider-neutral paths, missing-value and JSON-null behavior, safe conversions, and captured-value parameterization.
- PostgreSQL 18 integration coverage alongside the shared scalar contract suite.

### Compatibility

- .NET 10.
- EF Core relational `>= 10.0.9` and `< 11.0.0`.
- Npgsql EF Core provider `>= 10.0.3` and `< 11.0.0`.
- PostgreSQL 16 or later.

### Package validation

The preview package is built as a real `.nupkg` and inspected for aligned preview versioning, dependency ranges, release notes, package metadata, its consumer README, and the expected runtime DLL and XML documentation.

Release validation also requires the unit suite, PostgreSQL integration suite, release build, package creation, dependency vulnerability audit, and clean diff checks to pass.

### Limitations

This is a preview rather than a stable release. Stable 1.0 guarantees, collection-query documentation, performance tuning guidance, deployment guidance, and automated publishing are outside this release's scope.
