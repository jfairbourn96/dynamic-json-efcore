# Security review: Story 46 portable scalar JSON paths

## Scope and status

This point-in-time review analyzes the provider-neutral scalar JSON query architecture through Story 46. It covers:

- public scalar query markers;
- portable scalar JSON path construction and parsing;
- SQL Server scalar translation and registration;
- generated SQL parameterization;
- malformed and hostile path input; and
- relevant direct and transitive NuGet dependencies.

Review date: July 17, 2026.

This is a point-in-time engineering assessment, not a guarantee that the package or its dependencies will remain free of vulnerabilities. Repeat the dependency and code review when query capabilities, providers, EF Core versions, or package dependencies change.

## Threat model

The scalar query API may receive property names or complete paths derived from runtime metadata, tenant configuration, HTTP search input, or other data that is not inherently trusted.

Relevant threats include:

- SQL injection through a path or property name;
- JSON path injection that changes the intended traversal;
- access to collection syntax or provider-specific behavior outside the portable contract;
- malformed escaping that different providers interpret inconsistently;
- denial of service through pathological parsing behavior;
- accidental client-side execution of query-only methods;
- provider-specific dependencies or SQL leaking into core; and
- vulnerable direct or transitive dependencies.

Authorization remains an application responsibility. A syntactically safe path does not decide whether a caller is authorized to query a particular dynamic field.

## Security properties

### SQL construction and parameterization

`DynamicJsonFunctions` marker calls remain in EF Core expression trees. The SQL Server provider constructs `JSON_VALUE` and `TRY_CONVERT` expressions through EF Core's `ISqlExpressionFactory`; it does not concatenate user-controlled text into a SQL command.

Runtime path values are emitted as database parameters. Constant paths are validated and canonicalized by `DynamicJsonPath` and then returned to EF Core's expression factory for type mapping and SQL literal escaping.

The only `SqlFragmentExpression` values used by scalar translation are hard-coded SQL Server store types owned by the provider package:

```text
decimal(18, 4)
date
```

No user-controlled value is placed in a `SqlFragmentExpression`.

### Property-name escaping

Applications should construct runtime paths from exact property names:

```csharp
string path = DynamicJsonPath.FromProperty(runtimePropertyName);
```

Simple portable identifiers use dot notation. Every other property name becomes one double-quoted segment with JSON string escaping. Input that resembles path or SQL syntax therefore remains data rather than changing traversal or SQL structure.

Security regression coverage includes property names resembling:

```text
items[0]
*
strict $.secret
stage' OR 1=1--
```

Each value round-trips as one exact property name. The generated SQL test also verifies that the SQL-injection-shaped name is passed to `JSON_VALUE` through a parameter.

### Unsupported path syntax

The portable scalar contract rejects:

- array indexes;
- wildcards;
- recursive descent;
- filters and expressions;
- bracket notation;
- relative paths;
- provider modes such as SQL Server `strict`; and
- malformed, incomplete, or invalid JSON string escapes.

Invalid paths throw `DynamicJsonPathException`. This fail-closed behavior prevents constant expressions from silently acquiring provider-specific semantics.

Collection traversal must be introduced through separate provider-neutral operations such as a future `ArrayAny`; it must not be enabled by accepting array syntax in scalar paths.

### Parser behavior

`DynamicJsonPath` parses input with a single forward scan. It does not use regular expressions and therefore has no catastrophic-backtracking exposure. Time and memory consumption grow linearly with the supplied path and decoded property names.

The parser uses `System.Text.Json` to decode quoted property tokens and catches malformed JSON string syntax. Canonical output escapes quotation marks, backslashes, and control characters before paths reach a provider.

The library does not currently impose a maximum path length. Applications accepting arbitrary user input should apply their normal request and metadata size limits. Database providers may also impose parameter or JSON path length limits.

### Query-only execution

`DynamicJsonFunctions.Value`, `ValueDecimal`, and `ValueDate` throw `NotSupportedException` if invoked directly. They are intended only for EF Core expression trees and do not fall back to client-side JSON evaluation.

### Package boundary

`Dynamic.Json.EfCore` does not reference SQL Server or `Microsoft.EntityFrameworkCore.Relational`. SQL Server SQL, relational type mapping, translation, and service registration remain isolated in `Dynamic.Json.EfCore.SqlServer`.

This boundary reduces the risk that a future provider accidentally reuses SQL Server syntax or exposes provider-specific escape behavior through the portable API.

## Runtime-path compatibility constraint

The public LINQ marker signatures continue to accept `string` paths for backward compatibility. Once EF Core parameterizes a captured runtime string, a method translator can see the SQL parameter expression but not the original runtime value during translation.

Consequently:

- constant paths are validated by the provider translator;
- runtime paths constructed through `DynamicJsonPath` are validated before EF parameterization; and
- a caller that deliberately supplies an unvalidated raw runtime string bypasses core path validation, although EF still parameterizes it and prevents SQL injection.

Consumers should not concatenate runtime property names into paths or pass arbitrary raw path strings. Use `FromProperty`, `FromProperties`, `AppendProperty`, or `Normalize` at the input boundary.

This constraint should be reconsidered only as part of a deliberate public API versioning decision. It should not be addressed through provider-specific runtime syntax checks that would produce inconsistent cross-provider behavior.

## Dependency audit

The following command was run against the complete solution on July 17, 2026:

```powershell
dotnet list Dynamic.Json.EfCore.slnx package --vulnerable --include-transitive
```

NuGet reported no known vulnerable packages from the configured sources for:

- `Dynamic.Json.AspNetCore`;
- `Dynamic.Json.EfCore`;
- `Dynamic.Json.EfCore.SqlServer`;
- `Dynamic.Json.Search`;
- the unit test project; and
- the integration test project.

This result reflects advisories available from the configured NuGet sources at the time of the scan. CI or release workflows should repeat the audit rather than relying on this recorded result.

## Validation performed

The reviewed implementation passed:

- 105 unit tests;
- 14 Docker-free SQL Server registration, translation, path, and security tests;
- a full solution build with zero warnings and errors; and
- `git diff --check`.

A real SQL Server integration test verifies escaped and nested property filtering. It compiles in the current environment but requires Docker to execute.

## Residual risks and recommendations

### Application authorization

The library guarantees path syntax and translation behavior, not field-level authorization. Applications that restrict searchable fields must validate requested metadata fields before building a path.

### Input size

Path parsing is linear but unbounded. Applications should retain request-size and metadata-size limits appropriate to their deployment. A future shared maximum should be introduced only with an explicit compatibility contract.

### Provider consistency

Every new provider must consume the core path parser or reproduce its decoded property sequence exactly. Provider tests must include quoted properties, control escapes, unsupported syntax, and injection-shaped names.

### Collection translation

Future collection work expands the query pipeline and should receive a separate security review covering:

- deterministic fragment scopes and aliases;
- nested predicate capture;
- collection traversal limits;
- custom EF Core preprocessor or SQL generator service replacement;
- SQL expression ownership and user-controlled fragments;
- composition with ordinary EF Core translators;
- unsupported nested collection behavior; and
- denial-of-service potential from deeply nested predicates.

### Ongoing checks

Before each release:

1. Run the full unit and provider integration suites.
2. Run the NuGet vulnerability audit with transitive dependencies.
3. Review changes to SQL fragments, raw SQL, interceptors, query preprocessors, and SQL generators.
4. Confirm runtime property names are passed through `DynamicJsonPath` at application boundaries.
5. Reassess this document whenever EF Core or a database provider receives a major upgrade.
