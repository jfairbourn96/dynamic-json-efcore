# Scalar null and conversion contract

Every scalar provider must expose the same observable null behavior through the provider-neutral
`DynamicJsonFunctions` methods. This keeps an application's LINQ predicates portable even when the
underlying database uses different JSON operators and conversion functions.

## Required behavior

| Input condition | `Value` | `ValueDecimal` | `ValueDate` |
|---|---|---|---|
| Property is missing | `null` | `null` | `null` |
| Property contains JSON `null` | `null` | `null` | `null` |
| Database JSON document is `NULL` | `null` | `null` | `null` |
| Decimal text is invalid | Original text | `null` | Provider conversion result |
| Date text is invalid | Original text | Provider conversion result | `null` |
| Valid scalar value | Scalar text | Converted decimal | Converted date |

Invalid conversion must not raise a database conversion error. A provider may use its native safe
conversion operation or an equivalent expression, but the result exposed to EF Core must be nullable.
The exact set of strings accepted as decimal or date values follows the database provider's native
conversion rules; only failed-conversion behavior is portable.

SQL `NULL` comparison rules still apply after translation. For example, a predicate comparing a
missing value with a non-null constant does not match that row.

## SQL Server implementation

The SQL Server provider implements missing, JSON-null, and database-null behavior with nullable
`JSON_VALUE`. Decimal and date conversions use `TRY_CONVERT`, which returns SQL `NULL` instead of
raising an error for invalid input. These are the existing SQL Server semantics and remain unchanged.

Collection elements and application-level validation are outside this contract.
