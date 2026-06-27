using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamic.Employees.Data.Extensions;

public static class EmployeeDataExtensions
{
    public static IServiceCollection RegisterEmployeeDataServices(this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<DynamicEmployeeDbContext>(options => options.UseSqlServer(connectionString));

        return services;
    }
}