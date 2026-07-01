using DynamicEmployee.Core.Enums;

namespace EmployeeApi.Responses;

/// <summary>
/// A selectable option on a field of type <c>Select</c>.
/// </summary>
public class FieldOptionResponse
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// A single dynamic field definition belonging to an employee type.
/// </summary>
public class EmployeeTypeFieldResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public bool Required { get; set; }
    public List<FieldOptionResponse> Options { get; set; } = [];
    public int Order { get; set; }
}

/// <summary>
/// The API representation of an employee type, shaped for frontend consumption.
/// </summary>
public class EmployeeTypeResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ParentTypeId { get; set; }
    public List<EmployeeTypeFieldResponse> Fields { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
