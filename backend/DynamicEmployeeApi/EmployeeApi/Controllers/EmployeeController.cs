// TODO: Replace direct DbContext injection with a proper repository once
// the repository pattern is wired up. This controller exists solely to
// exercise the FieldValues JsonObject conversion and ValueComparer configs.

using Dynamic.Employees.Core.Models;
using Dynamic.Employees.Data;
using EmployeeApi.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
}
