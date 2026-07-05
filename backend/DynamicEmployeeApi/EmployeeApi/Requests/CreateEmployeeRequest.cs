using System.Text.Json.Nodes;

namespace EmployeeApi.Requests;

public class CreateEmployeeRequest
{
    public required string FirstName { get; set; } = string.Empty;
    public required string LastName { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public required DateOnly HireDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Department { get; set; }
    public Guid EmployeeTypeId { get; set; }
    public JsonObject FieldValues { get; set; } = new();
}
