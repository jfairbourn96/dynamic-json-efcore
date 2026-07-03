using Dynamic.Employees.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi;

public class EmployeeDbContext(DbContextOptions<EmployeeDbContext> options) 
    : BaseEmployeeDbContext(options)
{
}