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

## API Documentation

- Add Swagger/OpenAPI documentation back intentionally after choosing a package/version without known vulnerabilities.
- Document dynamic search query parameters, supported operators, field types, and error responses.
- Include examples for core employee filters and dynamic JSON field filters.

## NuGet Publishing

- Add shared package metadata for all publishable projects:
  - `Authors`
  - `RepositoryUrl`
  - `PackageTags`
  - `PackageReadmeFile`
  - `GenerateDocumentationFile`
  - license metadata
- Add a package-focused `README.md` to each NuGet package or configure a shared package README.
- Add a `LICENSE` file before publishing publicly.
- Decide the first package version, likely `0.1.0-preview.1`.
- Add GitHub Actions CI for pull requests:
  - restore
  - build
  - run `Dynamic.Json.EfCore.UnitTests`
  - optionally run package vulnerability audit
- Add GitHub Actions publish workflow for tags/releases:
  - run unit tests
  - pack `Dynamic.Json.EfCore`
  - pack `Dynamic.Json.EfCore.AspNetCore`
  - pack `Dynamic.Json.EfCore.SqlServer`
  - publish packages to NuGet using a repository secret such as `NUGET_API_KEY`
- Keep Docker/Testcontainers integration tests separate from the required NuGet publish path until they are stable in CI.
