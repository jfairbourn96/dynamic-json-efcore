namespace EmployeeApi.Requests;

/// <summary>
/// A selectable option within a <see cref="CreateEmployeeTypeFieldRequest"/> of type <c>Select</c>.
/// </summary>
public class FieldOptionRequest
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
