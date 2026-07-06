# TODO

## Docker-Backed Integration Tests

Add integration tests for provider-specific `Dynamic.Json.EfCore.*` behavior using Docker/Testcontainers.

### SQL Server

- Build out the existing `Dynamic.Json.EfCore.IntegrationTests` project.
- Use Testcontainers for .NET to start a SQL Server container during test setup.
- Configure EF Core with the container-generated SQL Server connection string.
- Create a small test table with a `JsonObject` property configured through `HasJsonConversion()`.
- Execute real queries against SQL Server to verify:
  - `DynamicJsonFunctions.Value`
  - `DynamicJsonFunctions.ValueDecimal`
  - `DynamicJsonFunctions.ValueDate`
  - filtering behavior
  - null or missing JSON property behavior
  - invalid decimal/date conversion behavior
  - `JsonObjectComparisonMode.Semantic`
  - `JsonObjectComparisonMode.Serialized`

### Provider Translation Tests

- Reintroduce SQL Server translation tests outside the unit test project.
- Use `ToQueryString()` to verify generated SQL for:
  - `JSON_VALUE`
  - `TRY_CONVERT(decimal(18, 4), JSON_VALUE(...))`
  - `TRY_CONVERT(date, JSON_VALUE(...))`
  - missing `UseDynamicJsonSqlServer()` registration behavior

### Future Providers

- Add equivalent Docker-backed tests when PostgreSQL support is introduced.
- Add equivalent integration tests when Newtonsoft/JObject support is introduced.
