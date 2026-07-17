# Portable scalar JSON path contract

`Dynamic.Json.EfCore` uses one provider-neutral path contract for scalar values. Applications can keep the same LINQ query intent when changing database providers, and provider packages receive an explicit subset to translate.

## Supported subset

A path starts with `$`, representing the current JSON document or future JSON fragment, followed by zero or more property segments:

```text
$
$.name
$.address.city
$."employee.name"
```

Unquoted property names are limited to ASCII letters or `_` followed by ASCII letters, digits, or `_`. All other exact property names use a double-quoted segment with JSON string escaping:

```csharp
string simple = DynamicJsonPath.FromProperty("name");
// $.name

string escaped = DynamicJsonPath.FromProperty("employee.name");
// $."employee.name"

string nested = DynamicJsonPath.FromProperties("address", "zip-code");
// $.address."zip-code"
```

Use `FromProperty`, `FromProperties`, or `AppendProperty` for runtime metadata. Do not construct paths by concatenating untrusted or runtime property names.

`Normalize` validates an existing path and returns its canonical spelling. `ParseProperties` exposes the exact decoded property sequence for provider implementations.

## Unsupported syntax

The scalar contract rejects:

- array indexes such as `$[0]` or `$.items[0]`;
- wildcards and recursive descent;
- filters and expressions;
- provider modes or prefixes such as SQL Server `strict`;
- bracket-quoted properties;
- relative paths; and
- malformed or incomplete quoted segments.

Violations throw `DynamicJsonPathException`. Provider translators validate constant paths through this core contract. Runtime paths should be created through `DynamicJsonPath` so they are validated before EF Core parameterizes the value.

Collection traversal will add separate query operations rather than expanding the scalar path grammar. The `$` root and property-segment rules are intentionally reusable for paths evaluated against a future `JsonFragment`.

## Provider responsibility

Providers translate the decoded property sequence into their database representation. They must not silently accept unsupported provider-specific syntax through the portable API. SQL Server retains its existing `JSON_VALUE` translation and receives the canonical path for constant expressions.
