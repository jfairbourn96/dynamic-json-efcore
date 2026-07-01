using Dynamic.Employees.Core.Interfaces;
using Dynamic.Employees.Core.Models;
using EmployeeApi.Requests;
using EmployeeApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApi.Controllers;

/// <summary>
/// Manages employee types and their dynamic field schemas.
/// </summary>
[ApiController]
[Route("api/employee-types")]
public class EmployeeTypeController : ControllerBase
{
    private readonly IEmployeeTypeRepository _repo;

    public EmployeeTypeController(IEmployeeTypeRepository repo)
    {
        _repo = repo;
    }

    /// <summary>Returns all employee types.</summary>
    /// <returns>A list of all employee types.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<EmployeeTypeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        List<EmployeeType> types = await _repo.GetAllAsync();
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
        EmployeeType? type = await _repo.GetByIdAsync(id);

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
        EmployeeType type = new()
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Fields = request.Fields.Select(ToField).ToList(),
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
        };

        await _repo.AddAsync(type);
        await _repo.SaveAsync();

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
        EmployeeType? type = await _repo.GetByIdAsync(id);

        if (type is null)
        {
            return NotFound();
        }

        type.Name = request.Name;
        type.Description = request.Description;
        type.Fields = request.Fields.Select(ToField).ToList();
        type.UpdatedDate = DateTime.UtcNow;

        await _repo.UpdateAsync(type);
        await _repo.SaveAsync();

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
        EmployeeType? type = await _repo.GetByIdAsync(id);

        if (type is null)
        {
            return NotFound();
        }

        _repo.Delete(type);
        await _repo.SaveAsync();

        return NoContent();
    }

    private static EmployeeTypeField ToField(CreateEmployeeTypeFieldRequest f) => new()
    {
        Id = Guid.NewGuid(),
        Name = f.Name,
        DisplayName = f.Label,
        FieldType = f.FieldType,
        Required = f.Required,
        Options = f.Options.Select(o => new FieldOption { Label = o.Label, Value = o.Value }).ToList(),
        Order = f.Order,
    };

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
        Label = field.DisplayName,
        FieldType = field.FieldType,
        Required = field.Required,
        Options = field.Options.Select(o => new FieldOptionResponse { Label = o.Label, Value = o.Value }).ToList(),
        Order = field.Order,
    };
}
