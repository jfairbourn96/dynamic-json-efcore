using DynamicEmployee.Core.Enums;

namespace EmployeeApi.Requests;

/// <summary>
/// Describes a single dynamic field to be created on an employee type.
/// </summary>
public class CreateEmployeeTypeFieldRequest
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public bool Required { get; set; }
    public List<FieldOptionRequest> Options { get; set; } = [];
    public int Order { get; set; }
}
