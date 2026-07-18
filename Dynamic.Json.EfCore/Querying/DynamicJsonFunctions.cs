using System.Text.Json.Nodes;

namespace Dynamic.Json.EfCore.Querying;

/// <summary>
/// Provider-neutral JSON query functions for use in EF Core LINQ expressions.
/// </summary>
/// <remarks>
/// These methods define query intent and are not evaluated in .NET. Database provider packages
/// translate them into provider-specific SQL. Provider implementations discover the supported
/// methods through <see cref="DynamicJsonScalarMethods" />.
/// </remarks>
/// <example>
/// Use the marker methods inside an EF Core LINQ query after registering the matching database
/// provider package:
/// <code>
/// IQueryable&lt;Employee&gt; matchingEmployees = dbContext.Employees
///     .Where(employee =&gt;
///         DynamicJsonFunctions.Value(employee.FieldValues, "$.department") == "Engineering" &amp;&amp;
///         DynamicJsonFunctions.ValueDecimal(employee.FieldValues, "$.yearsOfService") &gt;= 5m);
///
/// List&lt;Employee&gt; employees = await matchingEmployees.ToListAsync();
/// </code>
/// The marker methods must remain in the expression tree; calling them directly in application
/// code throws <see cref="NotSupportedException" />.
/// </example>
public static class DynamicJsonFunctions
{
    /// <summary>
    /// Reads a scalar JSON value as text from the supplied JSON path.
    /// </summary>
    /// <param name="json">The JSON document expression to read from.</param>
    /// <param name="path">A property-only path that follows the <see cref="DynamicJsonPath" /> contract.</param>
    /// <returns>
    /// The scalar JSON value as text, or <see langword="null" /> when the property is missing,
    /// the JSON value is <see langword="null" />, or the database JSON document is <see langword="null" />.
    /// </returns>
    /// <example>
    /// <code>
    /// IQueryable&lt;Employee&gt; engineers = dbContext.Employees.Where(employee =&gt;
    ///     DynamicJsonFunctions.Value(employee.FieldValues, "$.department") == "Engineering");
    /// </code>
    /// </example>
    public static string? Value(JsonObject json, string path)
        => throw new NotSupportedException("Only for use in EF Core queries.");

    /// <summary>
    /// Reads and converts a scalar JSON value to a nullable decimal.
    /// </summary>
    /// <param name="json">The JSON document expression to read from.</param>
    /// <param name="path">A property-only path that follows the <see cref="DynamicJsonPath" /> contract.</param>
    /// <returns>
    /// The converted decimal value, or <see langword="null" /> when conversion fails, the property
    /// is missing, the JSON value is <see langword="null" />, or the database JSON document is
    /// <see langword="null" />.
    /// </returns>
    /// <example>
    /// <code>
    /// IQueryable&lt;Employee&gt; experiencedEmployees = dbContext.Employees.Where(employee =&gt;
    ///     DynamicJsonFunctions.ValueDecimal(employee.FieldValues, "$.yearsOfService") &gt;= 5m);
    /// </code>
    /// </example>
    public static decimal? ValueDecimal(JsonObject json, string path)
        => throw new NotSupportedException("Only for use in EF Core queries.");

    /// <summary>
    /// Reads and converts a scalar JSON value to a nullable date.
    /// </summary>
    /// <param name="json">The JSON document expression to read from.</param>
    /// <param name="path">A property-only path that follows the <see cref="DynamicJsonPath" /> contract.</param>
    /// <returns>
    /// The converted date value, or <see langword="null" /> when conversion fails, the property is
    /// missing, the JSON value is <see langword="null" />, or the database JSON document is
    /// <see langword="null" />.
    /// </returns>
    /// <example>
    /// <code>
    /// DateOnly cutoff = new(2025, 1, 1);
    /// IQueryable&lt;Employee&gt; recentHires = dbContext.Employees.Where(employee =&gt;
    ///     DynamicJsonFunctions.ValueDate(employee.FieldValues, "$.hireDate") &gt;= cutoff);
    /// </code>
    /// </example>
    public static DateOnly? ValueDate(JsonObject json, string path)
        => throw new NotSupportedException("Only for use in EF Core queries.");
}
