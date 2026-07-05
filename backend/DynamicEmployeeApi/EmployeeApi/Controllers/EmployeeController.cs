// TODO: Replace direct DbContext injection with a proper repository once
// the repository pattern is wired up. This controller exists solely to
// exercise the FieldValues JsonObject conversion and ValueComparer configs.

using System.Text.Json.Nodes;
using Dynamic.Employees.Core.Models;
using Dynamic.Employees.Data;
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

    public EmployeeController(BaseEmployeeDbContext db)
    {
        _db = db;
    }

    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? firstName,
        [FromQuery] string? lastName,
        [FromQuery] string? email,
        [FromQuery] string? department,
        [FromQuery] Guid? employeeTypeId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<Employee> query = _db.Employee
            .AsNoTracking()
            .Include(e => e.EmployeeType)
                .ThenInclude(et => et!.Fields);

        if (!string.IsNullOrWhiteSpace(firstName))
        {
            query = query.Where(e => e.FirstName.Contains(firstName.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(lastName))
        {
            query = query.Where(e => e.LastName.Contains(lastName.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            query = query.Where(e => e.Email.Contains(email.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            query = query.Where(e => e.Department != null && e.Department.Contains(department.Trim()));
        }

        if (employeeTypeId.HasValue)
        {
            query = query.Where(e => e.EmployeeTypeId == employeeTypeId.Value);
        }

        Dictionary<string, string> fieldValueFilters = GetFieldValueFilters(Request.Query);
        List<Employee> employees = await query.ToListAsync();

        if (fieldValueFilters.Count > 0)
        {
            employees = employees
                .Where(employee => MatchesFieldValueFilters(employee, fieldValueFilters))
                .ToList();
        }

        int totalCount = employees.Count;
        List<Employee> page = employees
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            Items = page,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        });
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

    private static Dictionary<string, string> GetFieldValueFilters(IQueryCollection query)
    {
        const string prefix = "fieldValues.";
        Dictionary<string, string> filters = new(StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<string, StringValues> parameter in query)
        {
            if (!parameter.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string fieldName = parameter.Key[prefix.Length..];
            string? value = parameter.Value.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(fieldName) && !string.IsNullOrWhiteSpace(value))
            {
                filters[fieldName] = value.Trim();
            }
        }

        return filters;
    }

    private static bool MatchesFieldValueFilters(Employee employee, Dictionary<string, string> filters)
    {
        foreach (KeyValuePair<string, string> filter in filters)
        {
            if (!employee.FieldValues.TryGetPropertyValue(filter.Key, out JsonNode? value) || value is null)
            {
                return false;
            }

            string actualValue = GetSearchableJsonValue(value);

            if (!actualValue.Contains(filter.Value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static string GetSearchableJsonValue(JsonNode value)
    {
        if (value is JsonValue jsonValue && jsonValue.TryGetValue<string>(out string? stringValue))
        {
            return stringValue;
        }

        return value.ToJsonString();
    }
}
