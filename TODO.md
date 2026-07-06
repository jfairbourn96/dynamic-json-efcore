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

### Completed

- [x] Add shared package metadata for all publishable projects:
  - `Authors`
  - `RepositoryUrl`
  - `PackageTags`
  - `PackageReadmeFile`
  - `GenerateDocumentationFile`
  - license metadata
- [x] Configure the shared `README.md` as the NuGet package README.
- [x] Add a `LICENSE` file before publishing publicly.
- [x] Decide the first package version: `0.1.0-preview.1`.
- [x] Add GitHub Actions CI for pull requests:
  - restore
  - build
  - run `Dynamic.Json.EfCore.UnitTests`
  - run package vulnerability audit
  - pack packages
- [x] Add GitHub Actions publish workflow for tags/releases:
  - run unit tests
  - pack `Dynamic.Json.EfCore`
  - pack `Dynamic.Json.EfCore.AspNetCore`
  - pack `Dynamic.Json.EfCore.SqlServer`
  - publish packages to NuGet using `NUGET_API_KEY`
- [x] Keep Docker/Testcontainers integration tests separate from the required NuGet publish path until they are stable in CI.

### Remaining Release Setup

- [ ] Add `NUGET_API_KEY` as a GitHub Actions repository secret.
- [ ] Confirm the NuGet package owner/profile information before the first publish.
- [ ] Create the first release tag when ready, likely `v0.1.0-preview.1`.
- [ ] Consider adding symbol packages and SourceLink after the first preview package flow is proven.
