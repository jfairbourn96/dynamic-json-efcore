# TODO

## 1. Add validation for employee types and employees

- [ ] Add DataAnnotations to `CreateEmployeeRequest`
  - [ ] Require `FirstName`, `LastName`, and `Email`
  - [ ] Add max lengths for `FirstName`, `LastName`, `Email`, and `Department`
  - [ ] Add `[EmailAddress]` to `Email`
  - [ ] Implement `IValidatableObject` to reject `Guid.Empty` for `EmployeeTypeId`
  - [ ] Implement `IValidatableObject` to reject `EndDate` values before `HireDate`

- [ ] Add DataAnnotations to `CreateEmployeeTypeRequest`
  - [ ] Require `Name`
  - [ ] Add max lengths for `Name` and `Description`
  - [ ] Require at least one field with `[MinLength(1)]`
  - [ ] Implement `IValidatableObject` to reject duplicate field names, case-insensitively

- [ ] Add DataAnnotations to `CreateEmployeeTypeFieldRequest`
  - [ ] Require `Name` and `Label`
  - [ ] Add max lengths for `Name` and `Label`
  - [ ] Add a non-negative `[Range]` validation for `Order`
  - [ ] Implement `IValidatableObject` so `Select` fields require at least one option
  - [ ] Implement `IValidatableObject` so non-`Select` fields cannot define options

- [ ] Add DataAnnotations to `FieldOptionRequest`
  - [ ] Require `Label` and `Value`
  - [ ] Add reasonable max lengths for `Label` and `Value`

- [ ] Keep dynamic `FieldValues` schema validation in service-level logic
  - [ ] Validate `EmployeeTypeId` exists before creating an employee
  - [ ] Validate required dynamic fields are present
  - [ ] Validate dynamic values match the selected employee type field schema

- [ ] Add or update tests for invalid request payloads
  - [ ] Missing required employee fields returns `400`
  - [ ] Empty `EmployeeTypeId` returns `400`
  - [ ] `EndDate` before `HireDate` returns `400`
  - [ ] Duplicate employee type field names return `400`
  - [ ] Invalid select-field options return `400`

## 2. Refactor employee search to use GET query parameters

- [x] Change frontend employee search API from `POST /employees/search` to `GET /employees/search`
- [x] Send core filters as query parameters
- [x] Send pagination as `pageNumber` and `pageSize` query parameters
- [x] Send dynamic field filters using `fieldValues.<fieldName>` query parameters
- [x] Add backend `GET /api/employees/search` endpoint that reads filters from query params

## 3. Update frontend employee search filters for typed query operators

- [ ] Update `DynamicSearch` core text filters
  - [ ] Keep `Email` as a plain text filter
  - [ ] Add operator dropdowns for `First Name`, `Last Name`, and `Department`
  - [ ] Support `Contains`, `Starts With`, and `Exact Match`
  - [ ] Serialize text filters as `fieldName_contains=value`, `fieldName_startsWith=value`, or `fieldName_exact=value`

- [ ] Add a core `Hire Date` range filter
  - [ ] Add optional start date input
  - [ ] Add optional end date input
  - [ ] Serialize as `hireDate_startDate=value` and `hireDate_endDate=value`

- [ ] Update dynamic text field filters
  - [ ] Add operator dropdowns for dynamic text fields
  - [ ] Support `Contains`, `Starts With`, and `Exact Match`
  - [ ] Serialize as `fieldName_contains=value`, `fieldName_startsWith=value`, or `fieldName_exact=value`

- [ ] Update dynamic date field filters
  - [ ] Replace single date/text filter with optional start/end date inputs
  - [ ] Serialize as `fieldName_startDate=value` and `fieldName_endDate=value`

- [ ] Update dynamic boolean field filters
  - [ ] Render boolean fields as checkboxes
  - [ ] Serialize checked values as `fieldName=true`
  - [ ] Serialize unchecked values as `fieldName=false`

- [ ] Update dynamic select field filters
  - [ ] Render select fields as dropdowns using `FieldOption` values
  - [ ] Serialize selected options as `fieldName=fieldOptionValue`

- [ ] Update dynamic number field filters
  - [ ] Add operator dropdown with `Less than`, `Less than or equal to`, `Equal to`, `Greater than`, and `Greater than or equal to`
  - [ ] Add numeric value input
  - [ ] Serialize as `fieldName_lt=value`, `fieldName_lte=value`, `fieldName=value`, `fieldName_gt=value`, or `fieldName_gte=value`

- [ ] Update frontend search serialization
  - [ ] Remove the old `fieldValues.<fieldName>` query parameter format
  - [ ] Build query params directly from the operator-aware search state
  - [ ] Keep `employeeTypeId`, `pageNumber`, and `pageSize` query params unchanged

- [ ] Verify frontend search changes
  - [ ] Run `npm.cmd run build`
  - [ ] Smoke-check generated query strings for core and dynamic filters
