using Dynamic.Employees.Core.Interfaces;
using Dynamic.Employees.Core.Models;
using EmployeeApi.Requests;

namespace EmployeeApi.Services;

/// <summary>
/// Implements business logic operations for employee types.
/// </summary>
public class EmployeeTypeService : IEmployeeTypeService
{
    private readonly IEmployeeTypeRepository _repository;

    public EmployeeTypeService(IEmployeeTypeRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<List<EmployeeType>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    /// <inheritdoc />
    public async Task<EmployeeType?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    /// <inheritdoc />
    public async Task<EmployeeType> CreateAsync(CreateEmployeeTypeRequest request)
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

        await _repository.AddAsync(type);
        await _repository.SaveAsync();

        return type;
    }

    /// <inheritdoc />
    public async Task<EmployeeType?> UpdateAsync(Guid id, CreateEmployeeTypeRequest request)
    {
        EmployeeType? type = await _repository.GetByIdAsync(id);

        if (type is null)
        {
            return null;
        }

        type.Name = request.Name;
        type.Description = request.Description;
        type.Fields = request.Fields.Select(ToField).ToList();
        type.UpdatedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(type);
        await _repository.SaveAsync();

        return type;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id)
    {
        EmployeeType? type = await _repository.GetByIdAsync(id);

        if (type is null)
        {
            return false;
        }

        _repository.Delete(type);
        await _repository.SaveAsync();

        return true;
    }

    private static EmployeeTypeField ToField(CreateEmployeeTypeFieldRequest f) => new()
    {
        Id = Guid.NewGuid(),
        Name = f.Name,
        Label = f.Label,
        FieldType = f.FieldType,
        Required = f.Required,
        Options = f.Options.Select(o => new FieldOption { Label = o.Label, Value = o.Value }).ToList(),
        Order = f.Order,
    };
}
