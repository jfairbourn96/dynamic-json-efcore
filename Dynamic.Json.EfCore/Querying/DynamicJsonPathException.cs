namespace Dynamic.Json.EfCore.Querying;

/// <summary>
/// Represents an invalid or unsupported provider-neutral JSON path.
/// </summary>
public sealed class DynamicJsonPathException : ArgumentException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicJsonPathException" /> class.
    /// </summary>
    /// <param name="message">A description of the path contract violation.</param>
    /// <param name="paramName">The name of the parameter containing the invalid path.</param>
    public DynamicJsonPathException(string message, string? paramName = null)
        : base(message, paramName)
    {
    }
}
