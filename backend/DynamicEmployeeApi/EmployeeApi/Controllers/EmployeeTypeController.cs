using Dynamic.Employees.Core.Models;
using EmployeeApi.Requests;
using EmployeeApi.Responses;
using EmployeeApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApi.Controllers;

/// <summary>
/// Manages employee types and their dynamic field schemas.
/// </summary>
[ApiController]
[Route("api/employee-types")]
public class EmployeeTypeController : ControllerBase
{
    private readonly IEmployeeTypeService _service;

    public EmployeeTypeController(IEmployeeTypeService service)
    {
        _service = service;
    }

    /// <summary>Returns all employee types.</summary>
    /// <returns>A list of all employee types.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<EmployeeTypeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        List<EmployeeType> types = await _service.GetAllAsync();
        return Ok(types.Select(ToResponse).ToList());
    }

    /// <summary>Returns the employee type with the specified <paramref name="id"/>.</summary>
    /// <param name="id">The employee type identifier.</param>
    /// <returns>The matching employee type, or 404 if not found.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        EmployeeType? type = await _service.GetByIdAsync(id);

        if (type is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(type));
    }

    /// <summary>Creates a new employee type.</summary>
    /// <param name="request">The employee type to create.</param>
    /// <returns>The created employee type.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeTypeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeTypeRequest request)
    {
        EmployeeType type = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = type.Id }, ToResponse(type));
    }

    /// <summary>Replaces the employee type with the specified <paramref name="id"/>.</summary>
    /// <param name="id">The employee type identifier.</param>
    /// <param name="request">The updated employee type data.</param>
    /// <returns>The updated employee type, or 404 if not found.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateEmployeeTypeRequest request)
    {
        EmployeeType? type = await _service.UpdateAsync(id, request);

        if (type is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(type));
    }

    /// <summary>Deletes the employee type with the specified <paramref name="id"/>.</summary>
    /// <param name="id">The employee type identifier.</param>
    /// <returns>204 No Content, or 404 if not found.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        bool deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private static EmployeeTypeResponse ToResponse(EmployeeType type) => new()
    {
        Id = type.Id.ToString(),
        Name = type.Name,
        Description = type.Description,
        ParentTypeId = null,
        Fields = type.Fields.Select(ToFieldResponse).ToList(),
        CreatedAt = type.CreatedDate,
        UpdatedAt = type.UpdatedDate,
    };

    private static EmployeeTypeFieldResponse ToFieldResponse(EmployeeTypeField field) => new()
    {
        Id = field.Id.ToString(),
        Name = field.Name,
        Label = field.Label,
        FieldType = field.FieldType,
        Required = field.Required,
        Options = field.Options.Select(o => new FieldOptionResponse { Label = o.Label, Value = o.Value }).ToList(),
        Order = field.Order,
    };
}
