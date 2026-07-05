// TODO: Replace direct DbContext injection with a proper repository once
// the repository pattern is wired up. This controller exists solely to
// exercise the FieldValues JsonObject conversion and ValueComparer configs.

using System.Globalization;
using System.Text.RegularExpressions;
using Dynamic.Employees.Core.Models;
using Dynamic.Employees.Data;
using Dynamic.Json.EfCore;
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
    private static readonly Regex SafeDynamicFieldName = new(
        "^[A-Za-z][A-Za-z0-9_]*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public EmployeeController(BaseEmployeeDbContext db)
    {
        _db = db;
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

        List<DynamicSearchFilter> dynamicFilters = GetDynamicSearchFilters(
            Request.Query,
            employeeType,
            errors);

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

    private static List<DynamicSearchFilter> GetDynamicSearchFilters(
        IQueryCollection parameters,
        EmployeeType? employeeType,
        List<string> errors)
    {
        List<DynamicSearchFilter> filters = [];
        bool needsEmployeeType = false;

        foreach (KeyValuePair<string, StringValues> parameter in parameters)
        {
            if (IsKnownNonDynamicQueryKey(parameter.Key))
            {
                continue;
            }

            string? value = parameter.Value.FirstOrDefault()?.Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (!TryParseDynamicFilterKey(parameter.Key, out string fieldName, out SearchOperator searchOperator))
            {
                errors.Add($"Unsupported search parameter '{parameter.Key}'.");
                continue;
            }

            needsEmployeeType = true;

            if (employeeType is null)
            {
                continue;
            }

            if (!SafeDynamicFieldName.IsMatch(fieldName))
            {
                errors.Add($"Dynamic field '{fieldName}' is not a valid field name.");
                continue;
            }

            EmployeeTypeField? field = employeeType.Fields
                .SingleOrDefault(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

            if (field is null)
            {
                errors.Add($"Dynamic field '{fieldName}' does not exist on employee type '{employeeType.Name}'.");
                continue;
            }

            if (!ValidateDynamicFilter(field, searchOperator, value, errors))
            {
                continue;
            }

            filters.Add(new DynamicSearchFilter(field, searchOperator, value));
        }

        if (needsEmployeeType && employeeType is null)
        {
            errors.Add("Dynamic field filters require a valid employeeTypeId query parameter.");
        }

        return filters;
    }

    private static IQueryable<Employee> ApplyDynamicFilters(
        IQueryable<Employee> query,
        IEnumerable<DynamicSearchFilter> filters)
    {
        foreach (DynamicSearchFilter filter in filters)
        {
            string path = ToJsonPath(filter.Field.Name);

            query = filter.Field.FieldType switch
            {
                FieldType.Text or FieldType.Address => ApplyDynamicTextFilter(query, path, filter),
                FieldType.Number => ApplyDynamicNumberFilter(query, path, filter),
                FieldType.Date => ApplyDynamicDateFilter(query, path, filter),
                FieldType.Boolean => ApplyDynamicBooleanFilter(query, path, filter),
                FieldType.Select => ApplyDynamicSelectFilter(query, path, filter),
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
            return query.Where(e => JsonDbFunctions.JsonValue(e.FieldValues, path) == filter.Value);
        }

        string pattern = BuildLikePattern(filter.Value, filter.Operator);
        return query.Where(e => EF.Functions.Like(JsonDbFunctions.JsonValue(e.FieldValues, path)!, pattern, @"\"));
    }

    private static IQueryable<Employee> ApplyDynamicNumberFilter(
        IQueryable<Employee> query,
        string path,
        DynamicSearchFilter filter)
    {
        decimal number = decimal.Parse(filter.Value, NumberStyles.Number, CultureInfo.InvariantCulture);

        return filter.Operator switch
        {
            SearchOperator.LessThan => query.Where(e => JsonDbFunctions.JsonValueDecimal(e.FieldValues, path) < number),
            SearchOperator.LessThanOrEqual => query.Where(e => JsonDbFunctions.JsonValueDecimal(e.FieldValues, path) <= number),
            SearchOperator.GreaterThan => query.Where(e => JsonDbFunctions.JsonValueDecimal(e.FieldValues, path) > number),
            SearchOperator.GreaterThanOrEqual => query.Where(e => JsonDbFunctions.JsonValueDecimal(e.FieldValues, path) >= number),
            _ => query.Where(e => JsonDbFunctions.JsonValueDecimal(e.FieldValues, path) == number),
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
            SearchOperator.StartDate => query.Where(e => JsonDbFunctions.JsonValueDate(e.FieldValues, path) >= date),
            SearchOperator.EndDate => query.Where(e => JsonDbFunctions.JsonValueDate(e.FieldValues, path) <= date),
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

        return query.Where(e => JsonDbFunctions.JsonValue(e.FieldValues, path) == jsonValue);
    }

    private static IQueryable<Employee> ApplyDynamicSelectFilter(
        IQueryable<Employee> query,
        string path,
        DynamicSearchFilter filter)
    {
        return query.Where(e => JsonDbFunctions.JsonValue(e.FieldValues, path) == filter.Value);
    }

    private static bool ValidateDynamicFilter(
        EmployeeTypeField field,
        SearchOperator searchOperator,
        string value,
        List<string> errors)
    {
        bool isValidOperator = field.FieldType switch
        {
            FieldType.Text or FieldType.Address => searchOperator is SearchOperator.Contains or SearchOperator.StartsWith or SearchOperator.Exact,
            FieldType.Number => searchOperator is SearchOperator.LessThan or SearchOperator.LessThanOrEqual or SearchOperator.Exact or SearchOperator.GreaterThan or SearchOperator.GreaterThanOrEqual,
            FieldType.Date => searchOperator is SearchOperator.StartDate or SearchOperator.EndDate,
            FieldType.Boolean => searchOperator == SearchOperator.Exact,
            FieldType.Select => searchOperator == SearchOperator.Exact,
            _ => false,
        };

        if (!isValidOperator)
        {
            errors.Add($"Search operator '{searchOperator}' is not valid for dynamic field '{field.Name}'.");
            return false;
        }

        if (field.FieldType == FieldType.Number &&
            !decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
        {
            errors.Add($"Dynamic field '{field.Name}' must be a valid number.");
            return false;
        }

        if (field.FieldType == FieldType.Date &&
            !DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            errors.Add($"Dynamic field '{field.Name}' must be a valid date.");
            return false;
        }

        if (field.FieldType == FieldType.Boolean &&
            !bool.TryParse(value, out _))
        {
            errors.Add($"Dynamic field '{field.Name}' must be true or false.");
            return false;
        }

        if (field.FieldType == FieldType.Select &&
            !field.Options.Any(o => o.Value.Equals(value, StringComparison.Ordinal)))
        {
            errors.Add($"Dynamic field '{field.Name}' has an invalid option value.");
            return false;
        }

        return true;
    }

    private static bool TryParseDynamicFilterKey(
        string key,
        out string fieldName,
        out SearchOperator searchOperator)
    {
        (string Suffix, SearchOperator Operator)[] suffixes =
        [
            ("_startsWith", SearchOperator.StartsWith),
            ("_startDate", SearchOperator.StartDate),
            ("_endDate", SearchOperator.EndDate),
            ("_contains", SearchOperator.Contains),
            ("_exact", SearchOperator.Exact),
            ("_lte", SearchOperator.LessThanOrEqual),
            ("_gte", SearchOperator.GreaterThanOrEqual),
            ("_lt", SearchOperator.LessThan),
            ("_gt", SearchOperator.GreaterThan),
        ];

        foreach ((string suffix, SearchOperator filterOperator) in suffixes)
        {
            if (key.EndsWith(suffix, StringComparison.Ordinal))
            {
                fieldName = key[..^suffix.Length];
                searchOperator = filterOperator;
                return !string.IsNullOrWhiteSpace(fieldName);
            }
        }

        fieldName = key;
        searchOperator = SearchOperator.Exact;
        return !string.IsNullOrWhiteSpace(fieldName);
    }

    private static bool IsKnownNonDynamicQueryKey(string key)
    {
        if (key is "employeeTypeId" or "pageNumber" or "pageSize" or "email")
        {
            return true;
        }

        if (key is "hireDate_startDate" or "hireDate_endDate")
        {
            return true;
        }

        return key.StartsWith("firstName_", StringComparison.Ordinal)
            || key.StartsWith("lastName_", StringComparison.Ordinal)
            || key.StartsWith("department_", StringComparison.Ordinal);
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

    private enum SearchOperator
    {
        Contains,
        StartsWith,
        Exact,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
        StartDate,
        EndDate,
    }

    private sealed record DynamicSearchFilter(
        EmployeeTypeField Field,
        SearchOperator Operator,
        string Value);
}
