# Changelog

## [Unreleased]

### Added

- `Dynamic.Json.Metadata` package for provider-neutral runtime metadata definitions.
- `JsonArray` runtime field metadata with explicit scalar element types.
- Metadata serialization and deserialization support through `System.Text.Json`.
- Metadata validation for field names, field types, array element types, select options, and duplicate fields.
- Metadata unit, serialization, and scalar regression tests.
- Runtime metadata and `JsonArray` documentation.

### Changed

- Separated runtime metadata responsibilities from `Dynamic.Json.Search` into the dedicated metadata package.
- Updated the solution, package documentation, and package tags for the new metadata boundary.
