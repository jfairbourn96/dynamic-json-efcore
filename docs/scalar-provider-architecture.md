# Scalar provider architecture

`Dynamic.Json.EfCore` defines scalar query intent; it does not define database SQL. This boundary lets an application keep the same LINQ API while selecting a separate database provider package.

## Responsibility boundary

The core package owns:

- the public `DynamicJsonFunctions.Value`, `ValueDecimal`, and `ValueDate` marker signatures;
- their CLR return types and the rule that marker methods are query-only;
- stable `MethodInfo` descriptors in `DynamicJsonScalarMethods` for provider discovery; and
- provider-neutral JSON mapping, tracking, and comparison helpers.

A provider package owns:

- recognizing the descriptors exposed by `DynamicJsonScalarMethods`;
- translating each supported marker to its database's SQL expressions and type mappings;
- registering translators with that database provider's EF Core services;
- all database-specific dependencies, SQL names, casts, operators, quoting, and diagnostics; and
- integration tests against the real database engine.

Core must not reference a provider package, `Microsoft.EntityFrameworkCore.Relational`, provider SQL types, or provider service-registration APIs. Provider packages may reference core and the EF Core relational/provider packages they implement. Applications opt in through a provider-package registration extension, such as `UseDynamicJsonSqlServer()`.

## Implementing another scalar provider

1. Create a separate provider assembly that references `Dynamic.Json.EfCore` and the target EF Core database provider.
2. Implement an EF Core method-call translator. Match calls against `DynamicJsonScalarMethods`; do not copy reflection lookups or add provider-specific marker methods to core.
3. Build SQL exclusively with the target provider's expression APIs. Do not place SQL fragments or provider dependencies in core.
4. Add an explicit provider registration extension. Registration must not happen automatically from core.
5. Test generated SQL and behavior against the real database. Core unit tests verify the shared marker boundary; provider integration tests verify translation and registration.

The portable JSON path subset and cross-provider null/conversion behavior are separate contracts. A provider must implement those contracts once defined; the extension boundary here does not grant provider-specific behavior to core or change the existing SQL Server translation.

## SQL Server compatibility

`Dynamic.Json.EfCore.SqlServer` consumes the shared descriptors and retains ownership of `JSON_VALUE`, `TRY_CONVERT`, SQL Server store types, and `UseDynamicJsonSqlServer()`. Moving method discovery to the shared boundary does not change the generated SQL or public registration API.
