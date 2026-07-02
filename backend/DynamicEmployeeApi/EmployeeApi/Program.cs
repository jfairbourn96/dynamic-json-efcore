using Dynamic.Employees.Data;
using Dynamic.Employees.Data.Extensions;
using EmployeeApi;
using EmployeeApi.Services;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.RegisterEmployeeDataServices<EmployeeDbContext>(connectionString);

// Register application services
builder.Services.AddScoped<IEmployeeTypeService, EmployeeTypeService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase));
    });

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
            builder.Configuration["Cors:AllowedOrigins"]?.Split(',') ?? ["http://localhost:5173"])
            .AllowAnyHeader()
            .AllowAnyMethod());
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using IServiceScope scope = app.Services.CreateScope();
    EmployeeDbContext db = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

app.Run();
