namespace Dynamic.Json.EfCore.Querying;

/// <summary>
/// Represents a JSON value produced while traversing a document in a database query.
/// </summary>
/// <remarks>
/// A fragment can contain an object, an array, a scalar, or JSON null. Instances only exist
/// inside query expressions and cannot be created or evaluated by application code.
/// </remarks>
public sealed class JsonFragment
{
    private JsonFragment()
    {
    }
}
