using Dynamic.Employees.Core.Models;

namespace Dynamic.Employees.Core.Interfaces;

/// <summary>
/// Provides data access operations for <see cref="EmployeeType"/> entities.
/// </summary>
public interface IEmployeeTypeRepository
{
    /// <summary>
    /// Returns all employee types.
    /// </summary>
    Task<List<EmployeeType>> GetAllAsync();

    /// <summary>
    /// Returns the employee type with the given <paramref name="id"/>, or <c>null</c> if not found.
    /// </summary>
    Task<EmployeeType?> GetByIdAsync(Guid id);

    /// <summary>
    /// Registers a new employee type for insertion on the next <see cref="SaveAsync"/> call.
    /// </summary>
    void Add(EmployeeType employeeType);

    /// <summary>
    /// Marks an existing employee type as modified so it is persisted on the next <see cref="SaveAsync"/> call.
    /// </summary>
    void Update(EmployeeType employeeType);

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    Task SaveAsync();
}
