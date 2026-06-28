using Dynamic.Employees.Core.Interfaces;
using Dynamic.Employees.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Dynamic.Employees.Data.Repositories;

/// <inheritdoc/>
public class EmployeeTypeRepository : IEmployeeTypeRepository
{
    private readonly BaseEmployeeDbContext _context;

    public EmployeeTypeRepository(BaseEmployeeDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<List<EmployeeType>> GetAllAsync()
        => await _context.EmployeeTypes.ToListAsync();

    /// <inheritdoc/>
    public async Task<EmployeeType?> GetByIdAsync(Guid id)
        => await _context.EmployeeTypes.FirstOrDefaultAsync(et => et.Id == id);

    /// <inheritdoc/>
    public void Add(EmployeeType employeeType)
        => _context.EmployeeTypes.Add(employeeType);

    /// <inheritdoc/>
    public void Update(EmployeeType employeeType)
        => _context.EmployeeTypes.Update(employeeType);

    /// <inheritdoc/>
    public Task SaveAsync()
        => _context.SaveChangesAsync();
}
