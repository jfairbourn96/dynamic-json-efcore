using Dynamic.Employees.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Dynamic.Employees.Data;

public class DynamicEmployeeDbContext(DbContextOptions<DynamicEmployeeDbContext> options) : DbContext(options)
{
    public DbSet<EmployeeTypeField> EmployeeTypeFields => Set<EmployeeTypeField>();
    public DbSet<EmployeeType> EmployeeTypes => Set<EmployeeType>();
    public DbSet<Employee> Employee => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DynamicEmployeeDbContext).Assembly);
    }
}