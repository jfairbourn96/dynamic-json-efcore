using DynamicEmployee.Core.Enums;

namespace DynamicEmployee.Core.Models;

public class EmployeeTypeField
{
    public Guid Id { get; set; }
    public Guid EmployeeTypeId { get; set; }
    public EmployeeType? EmployeeType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public bool Required { get; set; }
    public int Order { get; set; }
}