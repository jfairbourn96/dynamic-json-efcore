# Collection query architecture notes

## Purpose

This document preserves the architectural findings from the nested JSON array research so future collection-query stories can build on the experiment without copying its prototype implementation directly.

The research source is branch `research/nested-json-array-exploration_JAF`, commit `e77e344` (`Initial research for Dynamic Json Array functions`). Review that commit when implementation details or generated SQL examples are needed.

Collection translation remains separate from the provider-neutral scalar architecture. The scalar boundary established by Issue 45 is the baseline that collection work should extend without introducing database-specific behavior into `Dynamic.Json.EfCore`.

## Confirmed direction

The research demonstrated that a provider-neutral nested-array query can use a shape similar to:

```csharp
IQueryable<Employee> matchingEmployees = dbContext.Employees.Where(employee =>
    DynamicJsonFunctions.ArrayAny(
        employee.FieldValues,
        "$.addresses",
        address => DynamicJsonFunctions.Value(address, "$.city") == "Denver"));
```

The application expresses only JSON query intent. SQL Server can translate the collection traversal to `OPENJSON`, while another provider can translate the same expression to its own collection expansion mechanism.

`JsonFragment` is a useful provider-neutral concept for an object, array, scalar, or JSON null reached while traversing a JSON document. It should remain a query-only value and must not expose SQL aliases, provider types, or provider-specific path behavior.

## Responsibility boundaries

### Core package

`Dynamic.Json.EfCore` should own:

- public collection query intent, such as `ArrayAny`;
- the provider-neutral `JsonFragment` query type;
- scalar marker overloads that operate on a `JsonFragment`;
- stable method descriptors for every provider-translatable public marker;
- portable path, null, and conversion contracts shared by all providers; and
- query-only behavior when marker methods are invoked directly in .NET.

Core must not own:

- `OPENJSON`, `jsonb_array_elements`, or other database functions;
- SQL aliases, SQL fragments, relational type mappings, or provider expression types;
- provider service registration; or
- dependencies on an EF Core relational or database-provider package.

### Provider packages

A collection-capable provider package should own:

- recognition of core-owned method descriptors;
- provider-specific collection expansion and scalar extraction SQL;
- provider expression and type-mapping details;
- any required EF Core query-pipeline integration;
- provider registration through its existing options-builder extension; and
- generated-SQL and real-database integration tests.

For SQL Server, `Dynamic.Json.EfCore.SqlServer` owns `OPENJSON`, `JSON_VALUE`, `TRY_CONVERT`, aliases, store types, SQL generation, and registration through `UseDynamicJsonSqlServer()`.

## Method descriptor design

Issue 45 introduced `DynamicJsonScalarMethods` as the stable scalar translation boundary. Collection work should add a parallel boundary rather than returning to repeated reflection lookups in provider packages.

A likely organization is:

```text
DynamicJsonScalarMethods
  ValueFromDocument
  ValueDecimalFromDocument
  ValueDateFromDocument
  ValueFromFragment
  ValueDecimalFromFragment
  ValueDateFromFragment

DynamicJsonCollectionMethods
  ArrayAnyFromObject
  ArrayAnyFromArray
  ArrayAnyFromFragment
```

Final names can change, but overloaded marker signatures must be unambiguous to provider authors. Providers should compare incoming `MethodInfo` values with core-owned descriptors and should not repeat reflection by method name and parameter array.

The current scalar completeness test assumes every public method declared on `DynamicJsonFunctions` is a scalar marker. Collection work must deliberately evolve that test. Scalar and collection completeness should be verified independently against their explicit descriptor sets.

## Translation-only intermediate markers

The research prototype introduced public, IntelliSense-hidden methods named `Fragment` and `TranslatedArrayAny`. They allowed a query preprocessor to rewrite the consumer-facing lambda into expressions that the SQL Server translator and generator could recognize.

These methods proved the rewriting approach, but they should not be adopted as ordinary public consumer API. `EditorBrowsable(Never)` hides a method from typical IntelliSense but does not remove it from the supported public surface.

Before productization, choose an explicit design for intermediate markers:

- define a documented provider-neutral intermediate-marker contract;
- place intermediate markers in a dedicated advanced/hidden type; or
- redesign pipeline integration so providers do not require public intermediate methods.

Simply making the helpers `internal` and granting SQL Server `InternalsVisibleTo` access is not provider-neutral; it would couple core to a known provider and make third-party provider implementation harder.

## EF Core pipeline risk

The research replaced both `IQueryTranslationPreprocessorFactory` and `IQuerySqlGeneratorFactory`. It also subclassed SQL Server implementation types guarded by the `EF1001` internal-API warning.

This stayed within the SQL Server package and therefore respected the package dependency boundary, but it carries product risks:

- another library may replace the same EF Core services;
- replacing a service affects every query for the context;
- SQL Server internal APIs can change between EF Core releases;
- a custom SQL generator increases the surface area requiring regression coverage; and
- nested scopes and aliases must remain deterministic under composed queries.

Before committing to that implementation, investigate whether supported EF Core extension points or provider expression APIs can represent the necessary table-valued collection expansion. If service replacement remains necessary, document it as a provider limitation and protect it with compatibility tests.

## Path contract requirements

Issue 46 should define paths in a way that supports the later collection shapes without implementing collections itself. The contract should answer:

- whether root documents and `JsonFragment` values use the same syntax;
- whether `$` means the current document or current fragment root;
- how nested properties are represented;
- how dots, quotes, brackets, and other special characters in property names are escaped;
- whether array indexes are supported or explicitly excluded;
- whether wildcards, recursive descent, filters, and provider-specific operators are rejected;
- whether paths must be constants or may be query parameters; and
- whether unsupported paths are rejected by provider-neutral validation or during provider translation.

SQL Server's ability to pass a path to `OPENJSON(document, path)` is an implementation detail, not the definition of the portable path contract.

## Null and conversion requirements

Issue 47 should establish behavior for scalar reads from both documents and future fragments. Collection work should reuse that contract for:

- missing properties inside an array element;
- JSON null elements and JSON null properties;
- database null documents;
- invalid decimal and date values; and
- predicates whose scalar extraction produces null.

Provider translation can differ, but observable LINQ results must follow the shared contract.

## Source conventions

Production implementation should follow these repository conventions:

- one class per file, including internal classes;
- XML documentation on all production classes and members, including private methods;
- code samples on public query APIs used to build EF LINQ expressions;
- no XML documentation requirement for test classes or test methods;
- provider-specific class names and files remain in the provider package; and
- public marker methods clearly state that they are query-only and throw when directly evaluated.

The research commit groups several classes into individual files. Split the preprocessor factory, preprocessor, visitors, SQL expressions, SQL generator factory, and SQL generator when turning the prototype into production code.

## Test strategy

### Core tests

- Every public scalar marker has exactly one scalar descriptor.
- Every public collection marker has exactly one collection descriptor.
- Descriptor signatures and return types remain stable.
- Document and fragment overloads are unambiguous.
- Direct invocation of every query-only marker throws.
- Core has no relational or database-provider assembly dependency.
- Portable paths are accepted or rejected consistently before provider behavior is considered.

### Docker-free provider tests

- Provider registration is required and remains idempotent.
- Each supported collection marker produces the expected provider SQL shape.
- Nested scalar reads use the provider's scalar translation inside collection predicates.
- Multiple arrays use distinct, deterministic aliases.
- Nested `ArrayAny` scopes do not capture or reuse the wrong fragment.
- Boolean combinations, parameters, projections, ordering, and paging compose correctly.
- Unrelated LINQ methods remain available to EF Core's normal translators.
- Any replaced EF Core service continues to generate ordinary non-JSON queries correctly.

### Real-provider integration tests

- Empty, populated, and null arrays behave according to the shared contract.
- Matching and nonmatching elements return the expected rows.
- Nested object properties inside elements can be filtered.
- Nested arrays are either translated correctly or rejected according to the supported scope.
- Missing, null, and failed-conversion values follow Issue 47 semantics.
- Parameterized paths and values behave according to the Issue 46 contract.

## Productization checklist

When a collection story begins:

1. Read this document and inspect research commit `e77e344`.
2. Confirm the story's supported collection shapes and explicit exclusions.
3. Reconcile the design with the completed path and null/conversion contracts.
4. Add provider-neutral public marker and fragment APIs in core.
5. Add core-owned scalar-fragment and collection method descriptors.
6. Update scalar and collection completeness tests independently.
7. Decide how translation-only intermediate markers remain provider-neutral without polluting normal consumer API.
8. Re-evaluate EF Core service replacement before copying the prototype pipeline.
9. Implement SQL Server behavior only in `Dynamic.Json.EfCore.SqlServer`.
10. Split every production class into its own file and add complete XML documentation.
11. Add Docker-free generated-SQL and registration tests.
12. Add real SQL Server behavior tests for supported semantics.
13. Verify ordinary scalar queries and non-JSON EF queries remain unchanged.

## Handoff summary

The research validated the feasibility of provider-neutral nested collection predicates. Its `JsonFragment` concept and application-facing `ArrayAny` shape fit the Issue 45 boundary. Its repeated reflection, public rewriting helpers, grouped classes, SQL-generator replacement, and use of provider internal APIs are prototype details to redesign or explicitly harden rather than copy unchanged.
