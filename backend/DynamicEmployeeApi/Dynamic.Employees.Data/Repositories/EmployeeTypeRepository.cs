using Dynamic.Employees.Core.Interfaces;
using Dynamic.Employees.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Dynamic.Employees.Data.Repositories;

/// <inheritdoc/>
public class EmployeeTypeRepository(BaseEmployeeDbContext context) : IEmployeeTypeRepository
{
    /// <inheritdoc/>
    public async Task<List<EmployeeType>> GetAllAsync() => await context.EmployeeTypes.ToListAsync();

    /// <inheritdoc/>
    public async Task<EmployeeType?> GetByIdAsync(Guid id)
        => await context.EmployeeTypes.FirstOrDefaultAsync(et => et.Id == id);

    /// <inheritdoc/>
    public async Task AddAsync(EmployeeType employeeType) => await context.EmployeeTypes.AddAsync(employeeType);

    /// <inheritdoc/>
    public async Task UpdateAsync(EmployeeType employeeType)
        => await Task.FromResult(context.EmployeeTypes.Update(employeeType));

    /// <inheritdoc/>
    public void Delete(EmployeeType employeeType) => context.EmployeeTypes.Remove(employeeType);

    /// <inheritdoc/>
    public Task SaveAsync() => context.SaveChangesAsync();
}
