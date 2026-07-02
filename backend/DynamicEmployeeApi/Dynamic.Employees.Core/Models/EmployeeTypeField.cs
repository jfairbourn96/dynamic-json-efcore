using System.ComponentModel.DataAnnotations.Schema;
using DynamicEmployee.Core.Enums;

namespace Dynamic.Employees.Core.Models;

public class EmployeeTypeField
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public bool Required { get; set; }
    [NotMapped]
    public List<FieldOption> Options { get; set; } = [];
    public int Order { get; set; }
}