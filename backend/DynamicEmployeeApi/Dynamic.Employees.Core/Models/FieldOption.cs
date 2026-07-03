namespace Dynamic.Employees.Core.Models;

/// <summary>
/// A single selectable option for a field of type <c>Select</c>.
/// </summary>
public class FieldOption
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
