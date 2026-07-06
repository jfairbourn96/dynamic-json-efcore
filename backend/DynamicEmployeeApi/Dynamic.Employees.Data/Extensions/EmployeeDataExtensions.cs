using Dynamic.Employees.Core.Interfaces;
using Dynamic.Employees.Data.Repositories;
using Dynamic.Json.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamic.Employees.Data.Extensions;

public static class EmployeeDataExtensions
{
    public static IServiceCollection RegisterEmployeeDataServices<TContext>(this IServiceCollection services,
        string connectionString) where TContext : BaseEmployeeDbContext
    {
        services.AddDbContext<TContext>(options =>
            options
                .UseSqlServer(connectionString,
                    x => x.MigrationsAssembly(typeof(TContext).Assembly.GetName().Name))
                .ReplaceService<IMethodCallTranslatorProvider, DynamicJsonSqlServerMethodCallTranslatorProvider>());

        // Allows controllers/services to inject BaseEmployeeDbContext directly.
        // Remove once the repository pattern is in place and nothing injects the DbContext directly.
        services.AddScoped<BaseEmployeeDbContext>(sp => sp.GetRequiredService<TContext>());

        services.AddScoped<IEmployeeTypeRepository, EmployeeTypeRepository>();

        return services;
    }
}
