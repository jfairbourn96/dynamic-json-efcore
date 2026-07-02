using Dynamic.Employees.Core.Models;
using EmployeeApi.Requests;

namespace EmployeeApi.Services;

/// <summary>
/// Provides business logic operations for employee types.
/// </summary>
public interface IEmployeeTypeService
{
    /// <summary>
    /// Retrieves all employee types.
    /// </summary>
    Task<List<EmployeeType>> GetAllAsync();

    /// <summary>
    /// Retrieves a specific employee type by ID.
    /// </summary>
    Task<EmployeeType?> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new employee type.
    /// </summary>
    Task<EmployeeType> CreateAsync(CreateEmployeeTypeRequest request);

    /// <summary>
    /// Updates an existing employee type.
    /// </summary>
    Task<EmployeeType?> UpdateAsync(Guid id, CreateEmployeeTypeRequest request);

    /// <summary>
    /// Deletes an employee type by ID.
    /// </summary>
    Task<bool> DeleteAsync(Guid id);
}
