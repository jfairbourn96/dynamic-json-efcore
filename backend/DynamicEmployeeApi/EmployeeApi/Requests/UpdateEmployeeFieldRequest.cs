using System.Text.Json.Nodes;

namespace EmployeeApi.Requests;

public class UpdateEmployeeFieldRequest
{
    public string FieldName { get; set; } = string.Empty;
    public JsonNode? Value { get; set; }
}
