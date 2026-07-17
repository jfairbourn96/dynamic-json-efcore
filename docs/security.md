# Security

## Purpose

This document defines the standing security expectations for `Dynamic.Json.EfCore` and its provider packages. Dated, story-specific findings are preserved under [`security-reviews`](security-reviews/) so dependency results, validation counts, assumptions, and residual risks remain tied to the code that was reviewed.

Security reviews support engineering decisions; they do not replace application authorization, deployment hardening, dependency monitoring, or database security controls.

## Security principles

### Treat query metadata as untrusted input

Property names, paths, filter values, and future collection predicates may originate from HTTP requests, tenant configuration, or runtime metadata. APIs must treat those values as data and must not concatenate them into SQL fragments.

Applications remain responsible for deciding which fields a caller may search. A valid path is not proof of authorization.

### Keep provider SQL out of core

`Dynamic.Json.EfCore` owns provider-neutral query intent and contracts. Database functions, SQL fragments, relational type mappings, service registration, and SQL generation belong in provider packages.

This dependency boundary is also a security boundary: provider-specific syntax must not become an undocumented escape hatch in the portable API.

### Prefer construction over concatenation

Runtime JSON property names should be passed through provider-neutral builders such as `DynamicJsonPath.FromProperty` or `DynamicJsonPath.FromProperties`. Builders must preserve an exact input name as one escaped property segment.

Consumers should not build paths by concatenating untrusted strings.

### Fail closed for unsupported syntax

Malformed paths and unsupported portable syntax should produce a documented exception rather than silently acquiring provider-specific meaning. New syntax must be added through an explicit cross-provider contract with unit and provider tests.

### Keep user input out of SQL fragments

Provider translators should use EF Core expression factories and database parameters. User-controlled values must never be placed in `SqlFragmentExpression`, raw SQL, identifiers, aliases, store-type declarations, or custom generator output.

### Avoid client-side query-marker execution

Query-only marker methods should throw when directly invoked. They must not silently evaluate JSON or predicates on the client when provider translation is unavailable.

## Application responsibilities

Applications using the library should:

- authorize searchable fields before constructing a query;
- build runtime paths through the provider-neutral path API;
- validate search operators and values against trusted metadata;
- apply appropriate HTTP request, metadata, path, and query-complexity limits;
- use least-privilege database credentials;
- avoid exposing generated SQL or detailed database errors to untrusted callers; and
- keep EF Core, provider packages, and the library on supported patched versions.

## Provider implementation requirements

Every provider must:

- recognize core-owned method descriptors rather than method names supplied at runtime;
- implement the documented path, null, and conversion contracts;
- parameterize values and route SQL construction through provider expression APIs;
- keep provider syntax and dependencies inside its own package;
- reject unsupported constant syntax consistently;
- test injection-shaped values, escaping, malformed input, and missing registration;
- verify ordinary non-JSON EF queries still compose correctly; and
- document any reliance on EF Core internal APIs or service replacement.

## Collection-query security requirements

Collection translation materially expands the attack surface. Before releasing collection queries, review:

- fragment scope and alias isolation;
- nested predicate capture and parameter ownership;
- maximum supported nesting or query complexity;
- unsupported nested collection behavior;
- user-controlled values reaching custom SQL expressions;
- EF Core preprocessor or SQL generator service replacement;
- compatibility with other translator plugins;
- denial-of-service potential from large or deeply nested predicates; and
- provider parity for paths, nulls, and conversions within fragments.

The implementation notes in [`collection-query-architecture-notes.md`](collection-query-architecture-notes.md) contain the broader collection productization checklist.

## Pull request security checklist

For changes to querying, translation, persistence, or provider registration:

- [ ] Identify every newly accepted untrusted value.
- [ ] Confirm values remain parameters or safely typed expressions.
- [ ] Confirm no user-controlled value reaches a SQL fragment or identifier.
- [ ] Add malformed, unsupported, escaped, and injection-shaped test cases.
- [ ] Verify missing provider registration fails safely.
- [ ] Verify core has no new relational or provider dependency.
- [ ] Assess input-size and query-complexity behavior.
- [ ] Document residual risks and application responsibilities.
- [ ] Run unit, generated-SQL, and applicable real-provider tests.
- [ ] Audit direct and transitive dependency advisories.

## Release security gates

Before each release:

1. Build the complete solution with warnings treated according to repository policy.
2. Run the full unit and provider integration suites.
3. Run the NuGet vulnerability audit with transitive dependencies:

   ```powershell
   dotnet list Dynamic.Json.EfCore.slnx package --vulnerable --include-transitive
   ```

4. Review changes involving raw SQL, SQL fragments, interceptors, query preprocessors, SQL generators, type mappings, or service replacement.
5. Confirm application-facing examples use safe runtime path construction.
6. Add or update a dated security review for any material query architecture or provider change.

## Review history

| Date | Scope | Review |
|---|---|---|
| 2026-07-17 | Story 46 portable scalar JSON paths and scalar architecture through Story 46 | [Review](security-reviews/2026-07-17-story-46-portable-scalar-json-paths.md) |
