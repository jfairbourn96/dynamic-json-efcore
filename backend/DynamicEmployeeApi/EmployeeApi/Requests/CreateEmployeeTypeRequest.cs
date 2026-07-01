namespace EmployeeApi.Requests;

/// <summary>
/// Request body for creating or replacing an employee type and its field schema.
/// </summary>
public class CreateEmployeeTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CreateEmployeeTypeFieldRequest> Fields { get; set; } = [];
}
