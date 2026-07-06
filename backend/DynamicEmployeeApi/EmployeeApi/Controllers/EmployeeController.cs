// TODO: Replace direct DbContext injection with a proper repository once
// the repository pattern is wired up. This controller exists solely to
// exercise the FieldValues JsonObject conversion and ValueComparer configs.

using System.Globalization;
using Dynamic.Employees.Core.Models;
using Dynamic.Employees.Data;
using Dynamic.Json.EfCore.AspNetCore;
using Dynamic.Json.EfCore.Querying;
using Dynamic.Json.EfCore.Search;
using DynamicEmployee.Core.Enums;
using EmployeeApi.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeeController : ControllerBase
{
    private readonly BaseEmployeeDbContext _db;
    private readonly IDynamicSearchQueryParser _dynamicSearchQueryParser;
    private static readonly DynamicSearchQueryParserOptions DynamicSearchParserOptions = CreateDynamicSearchParserOptions();

    public EmployeeController(
        BaseEmployeeDbContext db,
        IDynamicSearchQueryParser dynamicSearchQueryParser)
    {
        _db = db;
        _dynamicSearchQueryParser = dynamicSearchQueryParser;
    }

    // example query: http://localhost:5154/api/employees/search?firstName_startsWith=Jus&lastName_contains=bourn&department_exact=it&email=justin.fairbourn%40gmail.com&hireDate_startDate=2026-07-01&hireDate_endDate=2026-07-17&yearsOfExperience_gt=5&primaryLanguage_startsWith=C&graduationDate_startDate=2026-06-28&graduationDate_endDate=2026-07-17&level=se1&isFullstackCapable=false&employeeTypeId=af0976b0-83b1-4fb2-b58d-46cbf2bd7137&pageNumber=1&pageSize=20
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] Guid? employeeTypeId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        List<string> errors = [];

        IQueryable<Employee> query = _db.Employee
            .AsNoTracking()
            .Include(e => e.EmployeeType)
                .ThenInclude(et => et!.Fields);

        query = ApplyCoreFilters(query, Request.Query, errors);

        EmployeeType? employeeType = null;

        if (employeeTypeId.HasValue)
        {
            employeeType = await _db.EmployeeTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == employeeTypeId.Value);

            if (employeeType is null)
            {
                errors.Add("Employee type was not found.");
            }
            else
            {
                query = query.Where(e => e.EmployeeTypeId == employeeTypeId.Value);
            }
        }

        List<DynamicSearchFilter> dynamicFilters = GetDynamicSearchFilters(Request.Query, employeeType, errors);

        if (errors.Count > 0)
        {
            return BadRequest(new { Errors = errors });
        }

        query = ApplyDynamicFilters(query, dynamicFilters);

        int totalCount = await query.CountAsync();
        List<Employee> page = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            Items = page,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        });
    }

    private static IQueryable<Employee> ApplyCoreFilters(
        IQueryable<Employee> query,
        IQueryCollection parameters,
        List<string> errors)
    {
        query = ApplyCoreTextFilter(query, parameters, "firstName");
        query = ApplyCoreTextFilter(query, parameters, "lastName");
        query = ApplyCoreTextFilter(query, parameters, "department");

        string? email = GetQueryValue(parameters, "email");
        if (!string.IsNullOrWhiteSpace(email))
        {
            string pattern = BuildLikePattern(email, SearchOperator.Contains);
            query = query.Where(e => EF.Functions.Like(e.Email, pattern, @"\"));
        }

        string? hireDateStart = GetQueryValue(parameters, "hireDate_startDate");
        if (!string.IsNullOrWhiteSpace(hireDateStart))
        {
            if (DateOnly.TryParse(hireDateStart, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly startDate))
            {
                query = query.Where(e => e.HireDate >= startDate);
            }
            else
            {
                errors.Add("hireDate_startDate must be a valid date.");
            }
        }

        string? hireDateEnd = GetQueryValue(parameters, "hireDate_endDate");
        if (!string.IsNullOrWhiteSpace(hireDateEnd))
        {
            if (DateOnly.TryParse(hireDateEnd, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly endDate))
            {
                query = query.Where(e => e.HireDate <= endDate);
            }
            else
            {
                errors.Add("hireDate_endDate must be a valid date.");
            }
        }

        return query;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request)
    {
        Employee employee = new()
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            HireDate = request.HireDate,
            EndDate = request.EndDate ?? DateOnly.MinValue,
            Department = request.Department,
            EmployeeTypeId = request.EmployeeTypeId,
            FieldValues = request.FieldValues,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
        };

        _db.Employee.Add(employee);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee.Id);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Employee), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        Employee? employee = await _db.Employee
            .Include(e => e.EmployeeType)
                .ThenInclude(et => et!.Fields)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee is null)
        {
            return NotFound();
        }

        return Ok(employee);
    }

    [HttpPatch("{id:guid}/field")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateField(Guid id, [FromBody] UpdateEmployeeFieldRequest request)
    {
        Employee? employee = await _db.Employee.FindAsync(id);

        if (employee is null)
        {
            return NotFound();
        }

        employee.FieldValues[request.FieldName] = request.Value;
        employee.UpdatedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static IQueryable<Employee> ApplyCoreTextFilter(
        IQueryable<Employee> query,
        IQueryCollection parameters,
        string fieldName)
    {
        foreach (SearchOperator searchOperator in new[] { SearchOperator.Contains, SearchOperator.StartsWith, SearchOperator.Exact })
        {
            string key = BuildTextQueryKey(fieldName, searchOperator);
            string? value = GetQueryValue(parameters, key);

            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (searchOperator == SearchOperator.Exact)
            {
                query = query.Where(e => EF.Property<string?>(e, ToPropertyName(fieldName)) == value);
                continue;
            }

            string pattern = BuildLikePattern(value, searchOperator);
            query = query.Where(e => EF.Functions.Like(EF.Property<string>(e, ToPropertyName(fieldName)), pattern, @"\"));
        }

        return query;
    }

    private List<DynamicSearchFilter> GetDynamicSearchFilters(
        IQueryCollection parameters,
        EmployeeType? employeeType,
        List<string> errors)
    {
        if (employeeType is null)
        {
            if (_dynamicSearchQueryParser.HasDynamicSearchParameters(parameters, DynamicSearchParserOptions))
            {
                errors.Add("Dynamic field filters require a valid employeeTypeId query parameter.");
            }

            return [];
        }

        DynamicSearchFilterParseResult result = _dynamicSearchQueryParser.Parse(
            parameters,
            employeeType.Fields.Select(ToDynamicSearchField),
            DynamicSearchParserOptions);

        errors.AddRange(result.Errors.Select(error => FormatDynamicSearchParseError(error, employeeType.Name)));

        return [.. result.Filters];
    }

    private static IQueryable<Employee> ApplyDynamicFilters(
        IQueryable<Employee> query,
        IEnumerable<DynamicSearchFilter> filters)
    {
        foreach (DynamicSearchFilter filter in filters)
        {
            string path = ToJsonPath(filter.FieldName);

            query = filter.FieldType switch
            {
                DynamicSearchFieldType.Text => ApplyDynamicTextFilter(query, path, filter),
                DynamicSearchFieldType.Number => ApplyDynamicNumberFilter(query, path, filter),
                DynamicSearchFieldType.Date => ApplyDynamicDateFilter(query, path, filter),
                DynamicSearchFieldType.Boolean => ApplyDynamicBooleanFilter(query, path, filter),
                DynamicSearchFieldType.Select => ApplyDynamicSelectFilter(query, path, filter),
                _ => query,
            };
        }

        return query;
    }

    private static IQueryable<Employee> ApplyDynamicTextFilter(
        IQueryable<Employee> query,
        string path,
        DynamicSearchFilter filter)
    {
        if (filter.Operator == SearchOperator.Exact)
        {
            return query.Where(e => DynamicJsonFunctions.Value(e.FieldValues, path) == filter.Value);
        }

        string pattern = BuildLikePattern(filter.Value, filter.Operator);
        return query.Where(e => EF.Functions.Like(DynamicJsonFunctions.Value(e.FieldValues, path)!, pattern, @"\"));
    }

    private static IQueryable<Employee> ApplyDynamicNumberFilter(
        IQueryable<Employee> query,
        string path,
        DynamicSearchFilter filter)
    {
        decimal number = decimal.Parse(filter.Value, NumberStyles.Number, CultureInfo.InvariantCulture);

        return filter.Operator switch
        {
            SearchOperator.LessThan => query.Where(e => DynamicJsonFunctions.ValueDecimal(e.FieldValues, path) < number),
            SearchOperator.LessThanOrEqual => query.Where(e => DynamicJsonFunctions.ValueDecimal(e.FieldValues, path) <= number),
            SearchOperator.GreaterThan => query.Where(e => DynamicJsonFunctions.ValueDecimal(e.FieldValues, path) > number),
            SearchOperator.GreaterThanOrEqual => query.Where(e => DynamicJsonFunctions.ValueDecimal(e.FieldValues, path) >= number),
            _ => query.Where(e => DynamicJsonFunctions.ValueDecimal(e.FieldValues, path) == number),
        };
    }

    private static IQueryable<Employee> ApplyDynamicDateFilter(
        IQueryable<Employee> query,
        string path,
        DynamicSearchFilter filter)
    {
        DateOnly date = DateOnly.Parse(filter.Value, CultureInfo.InvariantCulture);

        return filter.Operator switch
        {
            SearchOperator.StartDate => query.Where(e => DynamicJsonFunctions.ValueDate(e.FieldValues, path) >= date),
            SearchOperator.EndDate => query.Where(e => DynamicJsonFunctions.ValueDate(e.FieldValues, path) <= date),
            _ => query,
        };
    }

    private static IQueryable<Employee> ApplyDynamicBooleanFilter(
        IQueryable<Employee> query,
        string path,
        DynamicSearchFilter filter)
    {
        bool boolValue = bool.Parse(filter.Value);
        string jsonValue = boolValue ? "true" : "false";

        return query.Where(e => DynamicJsonFunctions.Value(e.FieldValues, path) == jsonValue);
    }

    private static IQueryable<Employee> ApplyDynamicSelectFilter(
        IQueryable<Employee> query,
        string path,
        DynamicSearchFilter filter)
    {
        return query.Where(e => DynamicJsonFunctions.Value(e.FieldValues, path) == filter.Value);
    }

    private static string? GetQueryValue(IQueryCollection parameters, string key)
    {
        return parameters.TryGetValue(key, out StringValues values)
            ? values.FirstOrDefault()?.Trim()
            : null;
    }

    private static string BuildTextQueryKey(string fieldName, SearchOperator searchOperator)
    {
        string suffix = searchOperator switch
        {
            SearchOperator.Contains => "contains",
            SearchOperator.StartsWith => "startsWith",
            SearchOperator.Exact => "exact",
            _ => throw new ArgumentOutOfRangeException(nameof(searchOperator), searchOperator, null),
        };

        return $"{fieldName}_{suffix}";
    }

    private static string BuildLikePattern(string value, SearchOperator searchOperator)
    {
        string escapedValue = EscapeLikePattern(value.Trim());

        return searchOperator switch
        {
            SearchOperator.StartsWith => $"{escapedValue}%",
            _ => $"%{escapedValue}%",
        };
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal)
            .Replace("[", @"\[", StringComparison.Ordinal);
    }

    private static string ToJsonPath(string fieldName) => "$." + fieldName;

    private static DynamicSearchField ToDynamicSearchField(EmployeeTypeField field)
    {
        DynamicSearchFieldType fieldType = field.FieldType switch
        {
            FieldType.Text => DynamicSearchFieldType.Text,
            FieldType.Address => DynamicSearchFieldType.Text,
            FieldType.Number => DynamicSearchFieldType.Number,
            FieldType.Date => DynamicSearchFieldType.Date,
            FieldType.Boolean => DynamicSearchFieldType.Boolean,
            FieldType.Select => DynamicSearchFieldType.Select,
            _ => throw new ArgumentOutOfRangeException(nameof(field), field.FieldType, null),
        };

        return new DynamicSearchField(field.Name, fieldType, field.Options.Select(option => option.Value).ToArray());
    }

    private static DynamicSearchQueryParserOptions CreateDynamicSearchParserOptions()
    {
        DynamicSearchQueryParserOptions options = new();

        options.IgnoredKeys.UnionWith(
        [
            "employeeTypeId",
            "pageNumber",
            "pageSize",
            "email",
            "hireDate_startDate",
            "hireDate_endDate",
        ]);

        options.IgnoredKeyPrefixes.UnionWith(["firstName_", "lastName_", "department_"]);

        return options;
    }

    private static string FormatDynamicSearchParseError(
        DynamicSearchParseError error,
        string employeeTypeName)
    {
        return error.Code switch
        {
            DynamicSearchParseErrorCode.UnsupportedSearchParameter =>
                $"Unsupported search parameter '{error.QueryKey}'.",
            DynamicSearchParseErrorCode.InvalidFieldName =>
                $"Dynamic field '{error.FieldName}' is not a valid field name.",
            DynamicSearchParseErrorCode.UnknownField =>
                $"Dynamic field '{error.FieldName}' does not exist on employee type '{employeeTypeName}'.",
            DynamicSearchParseErrorCode.InvalidOperatorForFieldType =>
                $"Search operator '{error.Operator}' is not valid for dynamic field '{error.FieldName}'.",
            DynamicSearchParseErrorCode.InvalidNumberValue =>
                $"Dynamic field '{error.FieldName}' must be a valid number.",
            DynamicSearchParseErrorCode.InvalidDateValue =>
                $"Dynamic field '{error.FieldName}' must be a valid date.",
            DynamicSearchParseErrorCode.InvalidBooleanValue =>
                $"Dynamic field '{error.FieldName}' must be true or false.",
            DynamicSearchParseErrorCode.InvalidSelectOptionValue =>
                $"Dynamic field '{error.FieldName}' has an invalid option value.",
            _ => $"Unsupported search parameter '{error.QueryKey}'.",
        };
    }

    private static string ToPropertyName(string queryFieldName)
    {
        return queryFieldName switch
        {
            "firstName" => nameof(Employee.FirstName),
            "lastName" => nameof(Employee.LastName),
            "department" => nameof(Employee.Department),
            _ => throw new ArgumentOutOfRangeException(nameof(queryFieldName), queryFieldName, null),
        };
    }

}
