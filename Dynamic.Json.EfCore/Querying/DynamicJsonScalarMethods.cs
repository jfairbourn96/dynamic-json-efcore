using System.Reflection;
using System.Text.Json.Nodes;

namespace Dynamic.Json.EfCore.Querying;

/// <summary>
/// Identifies the provider-neutral scalar methods that database provider packages can translate.
/// </summary>
/// <remarks>
/// This class is the extension boundary between the core package and database provider packages.
/// Providers should compare method calls with these descriptors and keep SQL construction,
/// type mappings, and provider service registration in their own assemblies.
/// </remarks>
/// <example>
/// A provider method-call translator can recognize core query markers without repeating
/// reflection lookups:
/// <code>
/// public SqlExpression? Translate(
///     SqlExpression? instance,
///     MethodInfo method,
///     IReadOnlyList&lt;SqlExpression&gt; arguments,
///     IDiagnosticsLogger&lt;DbLoggerCategory.Query&gt; logger)
/// {
///     if (method == DynamicJsonScalarMethods.Value)
///     {
///         return TranslateTextValue(arguments);
///     }
///
///     return null;
/// }
/// </code>
/// </example>
public static class DynamicJsonScalarMethods
{
    /// <summary>Gets the method that reads a scalar JSON value as text.</summary>
    public static MethodInfo Value { get; } = GetMethod(nameof(DynamicJsonFunctions.Value));

    /// <summary>Gets the method that reads a scalar JSON value as a nullable decimal.</summary>
    public static MethodInfo ValueDecimal { get; } = GetMethod(nameof(DynamicJsonFunctions.ValueDecimal));

    /// <summary>Gets the method that reads a scalar JSON value as a nullable date.</summary>
    public static MethodInfo ValueDate { get; } = GetMethod(nameof(DynamicJsonFunctions.ValueDate));

    /// <summary>
    /// Resolves a scalar marker method using the common JSON document and path signature.
    /// </summary>
    /// <param name="name">The name of the marker method on <see cref="DynamicJsonFunctions" />.</param>
    /// <returns>The reflected method descriptor consumed by provider translators.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the requested marker method cannot be found with the expected signature.
    /// </exception>
    private static MethodInfo GetMethod(string name)
        => typeof(DynamicJsonFunctions).GetMethod(name, [typeof(JsonObject), typeof(string)])
            ?? throw new InvalidOperationException($"Could not find scalar JSON method '{name}'.");
}
