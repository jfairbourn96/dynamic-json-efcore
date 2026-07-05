using Dynamic.Employees.Core.Models;
using Dynamic.Json.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Dynamic.Employees.Data;

public abstract class BaseEmployeeDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<EmployeeType> EmployeeTypes => Set<EmployeeType>();
    public DbSet<Employee> Employee => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseEmployeeDbContext).Assembly);
        RegisterJsonDbFunctions(modelBuilder);
    }

    private static void RegisterJsonDbFunctions(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDbFunction(typeof(JsonDbFunctions).GetMethod(
                nameof(JsonDbFunctions.JsonValue),
                [typeof(System.Text.Json.Nodes.JsonObject), typeof(string)])!)
            .HasTranslation(args => JsonValue(args));

        modelBuilder
            .HasDbFunction(typeof(JsonDbFunctions).GetMethod(
                nameof(JsonDbFunctions.JsonValueDecimal),
                [typeof(System.Text.Json.Nodes.JsonObject), typeof(string)])!)
            .HasTranslation(args => TryConvert("decimal(18, 4)", JsonValue(args), typeof(decimal?)));

        modelBuilder
            .HasDbFunction(typeof(JsonDbFunctions).GetMethod(
                nameof(JsonDbFunctions.JsonValueDate),
                [typeof(System.Text.Json.Nodes.JsonObject), typeof(string)])!)
            .HasTranslation(args => TryConvert("date", JsonValue(args), typeof(DateOnly?)));
    }

    private static SqlExpression JsonValue(IReadOnlyList<SqlExpression> args) =>
        new SqlFunctionExpression(
            "JSON_VALUE",
            args,
            nullable: true,
            argumentsPropagateNullability: [true, true],
            type: typeof(string),
            typeMapping: null);

    private static SqlExpression TryConvert(string storeType, SqlExpression value, Type returnType) =>
        new SqlFunctionExpression(
            "TRY_CONVERT",
            [new SqlFragmentExpression(storeType), value],
            nullable: true,
            argumentsPropagateNullability: [false, true],
            type: returnType,
            typeMapping: null);
}
