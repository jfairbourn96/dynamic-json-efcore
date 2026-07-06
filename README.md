# dynamic-hr-in-ef-core
A dynamic HR records system PoC built on .NET 10 + React. Defines user types with custom field schemas, persists records as structured JSON, and renders forms and search filters dynamically at runtime.

## Dynamic.Json.EfCore

`Dynamic.Json.EfCore` contains provider-neutral helpers for storing and querying dynamic JSON values with EF Core. The base package includes `JsonObject` conversion and change-tracking support, while provider packages such as `Dynamic.Json.EfCore.SqlServer` translate the query functions into database-specific SQL.

### JSON comparison modes

`HasJsonConversion()` configures a `JsonObject` property to be stored as serialized JSON and tracked deeply by EF Core:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>(entity =>
    {
        entity.Property(e => e.DynamicFields).HasJsonConversion();
    });
}
```

By default, `HasJsonConversion()` uses `JsonObjectComparisonMode.Semantic`:

```csharp
entity.Property(e => e.DynamicFields)
    .HasJsonConversion(JsonObjectComparisonMode.Semantic);
```

Semantic comparison treats JSON objects as structured data rather than serialized text:

- Object property order does not affect equality.
- Nested objects are compared recursively.
- Arrays are compared in order, so array ordering remains significant.
- `null` equals `null`, but `null` does not equal a populated JSON object.
- `null` hashes to `0`.

For example, these objects are considered equal because they contain the same properties and values:

```json
{ "name": "Elsa", "role": "Queen" }
```

```json
{ "role": "Queen", "name": "Elsa" }
```

Arrays remain order-sensitive. These arrays are not equal:

```json
["Sven", "Olaf"]
```

```json
["Olaf", "Sven"]
```

Hash codes follow the same rules as equality. Object properties are hashed in sorted key order, so objects with the same data produce the same hash code even if their properties were inserted in a different order.

Snapshots are created by serializing and deserializing the `JsonObject`, producing a deep copy. That allows EF Core to detect changes made inside nested objects or arrays after the entity was loaded or saved.

For applications that prefer faster, property-order-sensitive comparison, use `JsonObjectComparisonMode.Serialized`:

```csharp
entity.Property(e => e.DynamicFields)
    .HasJsonConversion(JsonObjectComparisonMode.Serialized);
```

Serialized comparison compares the JSON text produced by serialization. It is simpler and faster, but these objects are considered different because their properties appear in different orders:

```json
{ "name": "Elsa", "role": "Queen" }
```

```json
{ "role": "Queen", "name": "Elsa" }
```

Use semantic comparison when JSON object property order should not matter. Use serialized comparison when raw comparison speed is more important and callers are comfortable with property-order-sensitive change detection.
