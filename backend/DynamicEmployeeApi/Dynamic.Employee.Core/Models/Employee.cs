using System.Text.Json.Nodes;

namespace DynamicEmployee.Core.Models;

public class Employee
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Department { get; set; }
    public Guid EmployeeTypeId { get; set; }
    public EmployeeType? EmployeeType { get; set; }
    public string FieldValuesJson { get; set; } = "{}";
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public JsonObject FieldValues { get; set; } = new();
}