using Dynamic.Employees.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Dynamic.Employees.Data;

public abstract class BaseEmployeeDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<EmployeeTypeField> EmployeeTypeFields => Set<EmployeeTypeField>();
    public DbSet<EmployeeType> EmployeeTypes => Set<EmployeeType>();
    public DbSet<Employee> Employee => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseEmployeeDbContext).Assembly);
    }
}