using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Dynamic.Json.EfCore.Querying;

/// <summary>
/// Creates and validates provider-neutral JSON paths composed exclusively of property segments.
/// </summary>
/// <remarks>
/// A portable path starts at <c>$</c> and continues with zero or more properties. Simple property
/// names use dot notation, such as <c>$.address.city</c>. Other names use a JSON-escaped quoted
/// segment, such as <c>$."employee.name"</c>. Array indexes, wildcards, filters, recursive descent,
/// and provider-specific syntax are not supported.
/// </remarks>
/// <example>
/// Build paths from runtime property names instead of concatenating them:
/// <code>
/// string cityPath = DynamicJsonPath.FromProperties("address", "city");
/// string dottedNamePath = DynamicJsonPath.FromProperty("employee.name");
///
/// IQueryable&lt;Employee&gt; matches = dbContext.Employees.Where(employee =&gt;
///     DynamicJsonFunctions.Value(employee.FieldValues, dottedNamePath) == "Zoey");
/// </code>
/// </example>
public static class DynamicJsonPath
{
    /// <summary>Gets the path representing the current JSON document or fragment.</summary>
    public const string Root = "$";

    /// <summary>
    /// Creates a portable path for a property on the root JSON value.
    /// </summary>
    /// <param name="propertyName">The exact JSON property name.</param>
    /// <returns>A canonical, escaped path for the property.</returns>
    public static string FromProperty(string propertyName)
        => AppendProperty(Root, propertyName);

    /// <summary>
    /// Creates a portable nested-property path from a sequence of exact JSON property names.
    /// </summary>
    /// <param name="propertyNames">The property names in traversal order.</param>
    /// <returns>A canonical, escaped path rooted at <c>$</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyNames" /> is null.</exception>
    public static string FromProperties(params string[] propertyNames)
    {
        ArgumentNullException.ThrowIfNull(propertyNames);

        string path = Root;
        foreach (string propertyName in propertyNames)
        {
            path = AppendProperty(path, propertyName);
        }

        return path;
    }

    /// <summary>
    /// Appends an exact property name to an existing portable path.
    /// </summary>
    /// <param name="path">An existing provider-neutral property path.</param>
    /// <param name="propertyName">The exact JSON property name to append.</param>
    /// <returns>A canonical path containing the additional escaped property segment.</returns>
    public static string AppendProperty(string path, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        string normalizedPath = Normalize(path);
        return normalizedPath + FormatProperty(propertyName);
    }

    /// <summary>
    /// Validates and canonicalizes a provider-neutral scalar JSON path.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <returns>The canonical path representing the same property traversal.</returns>
    /// <exception cref="DynamicJsonPathException">
    /// Thrown when the path is malformed or contains unsupported collection or provider syntax.
    /// </exception>
    public static string Normalize(string path)
    {
        IReadOnlyList<string> properties = ParseProperties(path);
        StringBuilder normalized = new(Root);

        foreach (string property in properties)
        {
            normalized.Append(FormatProperty(property));
        }

        return normalized.ToString();
    }

    /// <summary>
    /// Parses a portable scalar JSON path into its exact property names.
    /// </summary>
    /// <param name="path">The path to parse.</param>
    /// <returns>The ordered property names selected by the path.</returns>
    /// <exception cref="DynamicJsonPathException">
    /// Thrown when the path is malformed or contains unsupported syntax.
    /// </exception>
    public static IReadOnlyList<string> ParseProperties(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw InvalidPath("A JSON path must not be null or empty.");
        }

        if (path[0] != '$')
        {
            throw InvalidPath("A JSON path must start with the root token '$'.");
        }

        List<string> properties = [];
        int position = 1;

        while (position < path.Length)
        {
            if (path[position] != '.')
            {
                throw InvalidPath($"Unsupported JSON path syntax at position {position}. Only property segments are supported.");
            }

            position++;
            if (position == path.Length)
            {
                throw InvalidPath("A property segment must follow '.'.");
            }

            properties.Add(path[position] == '"'
                ? ParseQuotedProperty(path, ref position)
                : ParseSimpleProperty(path, ref position));
        }

        return properties.AsReadOnly();
    }

    /// <summary>
    /// Reads an unquoted property identifier from the current parser position.
    /// </summary>
    /// <param name="path">The complete JSON path.</param>
    /// <param name="position">The current position, advanced past the parsed property.</param>
    /// <returns>The parsed property name.</returns>
    private static string ParseSimpleProperty(string path, ref int position)
    {
        int start = position;
        if (!IsIdentifierStart(path[position]))
        {
            throw InvalidPath($"Property name at position {position} must be quoted because it is not a portable identifier.");
        }

        position++;
        while (position < path.Length && IsIdentifierPart(path[position]))
        {
            position++;
        }

        if (position < path.Length && path[position] != '.')
        {
            throw InvalidPath($"Unsupported JSON path syntax at position {position}. Only property segments are supported.");
        }

        return path[start..position];
    }

    /// <summary>
    /// Reads and JSON-decodes a quoted property from the current parser position.
    /// </summary>
    /// <param name="path">The complete JSON path.</param>
    /// <param name="position">The opening quote position, advanced past the closing quote.</param>
    /// <returns>The decoded property name.</returns>
    private static string ParseQuotedProperty(string path, ref int position)
    {
        int start = position;
        position++;
        bool escaped = false;

        while (position < path.Length)
        {
            char character = path[position++];
            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (character == '\\')
            {
                escaped = true;
                continue;
            }

            if (character == '"')
            {
                string token = path[start..position];
                string property;

                try
                {
                    property = JsonSerializer.Deserialize<string>(token)
                        ?? throw InvalidPath("A quoted property name cannot be null.");
                }
                catch (JsonException exception)
                {
                    throw new DynamicJsonPathException(
                        $"Quoted property at position {start} is not valid JSON string syntax: {exception.Message}",
                        "path");
                }

                if (position < path.Length && path[position] != '.')
                {
                    throw InvalidPath($"Unsupported JSON path syntax at position {position}. Only property segments are supported.");
                }

                return property;
            }
        }

        throw InvalidPath($"Quoted property at position {start} is missing its closing quote.");
    }

    /// <summary>
    /// Formats one exact property name using canonical simple or quoted notation.
    /// </summary>
    /// <param name="propertyName">The property name to format.</param>
    /// <returns>A property segment beginning with a dot.</returns>
    private static string FormatProperty(string propertyName)
    {
        if (propertyName.Length > 0 &&
            IsIdentifierStart(propertyName[0]) &&
            propertyName.Skip(1).All(IsIdentifierPart))
        {
            return "." + propertyName;
        }

        StringBuilder escaped = new(".\"");
        foreach (char character in propertyName)
        {
            switch (character)
            {
                case '"':
                    escaped.Append("\\\"");
                    break;
                case '\\':
                    escaped.Append("\\\\");
                    break;
                default:
                    if (char.IsControl(character))
                    {
                        escaped.Append("\\u");
                        escaped.Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        escaped.Append(character);
                    }

                    break;
            }
        }

        return escaped.Append('"').ToString();
    }

    /// <summary>
    /// Determines whether a character can begin a portable unquoted property identifier.
    /// </summary>
    /// <param name="character">The character to test.</param>
    /// <returns><see langword="true" /> for an ASCII letter or underscore; otherwise, <see langword="false" />.</returns>
    private static bool IsIdentifierStart(char character)
        => character == '_' || character is >= 'A' and <= 'Z' or >= 'a' and <= 'z';

    /// <summary>
    /// Determines whether a character can continue a portable unquoted property identifier.
    /// </summary>
    /// <param name="character">The character to test.</param>
    /// <returns><see langword="true" /> for an identifier-start character or ASCII digit; otherwise, <see langword="false" />.</returns>
    private static bool IsIdentifierPart(char character)
        => IsIdentifierStart(character) || character is >= '0' and <= '9';

    /// <summary>
    /// Creates the standard exception used for provider-neutral path contract violations.
    /// </summary>
    /// <param name="message">A description of the contract violation.</param>
    /// <returns>An exception associated with the path parameter.</returns>
    private static DynamicJsonPathException InvalidPath(string message)
        => new(message, "path");
}
