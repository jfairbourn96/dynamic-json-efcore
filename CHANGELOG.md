# Changelog

## [Unreleased]

### Added

- `Dynamic.Json.Metadata` package for provider-neutral runtime metadata definitions.
- `JsonArray` runtime field metadata with explicit scalar element types.
- Metadata serialization and deserialization support through `System.Text.Json`.
- Structured metadata validation results with stable error codes, metadata paths, and aggregated failures.
- Optional `ValidateAndThrow()` workflow with a metadata-specific exception containing the complete validation result.
- Metadata unit, serialization, and scalar regression tests.
- Runtime metadata and `JsonArray` documentation.

### Changed

- Separated runtime metadata responsibilities from `Dynamic.Json.Search` into the dedicated metadata package.
- Replaced constructor-thrown validation failures with explicit result-based validation.
- Simplified metadata serialization and validation to operate directly on field collections without a root wrapper type.
- Updated the solution, package documentation, and package tags for the new metadata boundary.
