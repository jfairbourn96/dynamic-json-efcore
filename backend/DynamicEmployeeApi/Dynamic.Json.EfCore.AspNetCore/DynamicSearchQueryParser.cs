using System.Globalization;
using System.Text.RegularExpressions;
using Dynamic.Json.EfCore.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Dynamic.Json.EfCore.AspNetCore;

/// <summary>
/// Parses ASP.NET Core query-string parameters into dynamic search filters.
/// </summary>
public static class DynamicSearchQueryParser
{
    private static readonly Regex SafeDynamicFieldName = new(
        "^[A-Za-z][A-Za-z0-9_]*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly (string Suffix, SearchOperator Operator)[] Suffixes =
    [
        ("_startsWith", SearchOperator.StartsWith),
        ("_startDate", SearchOperator.StartDate),
        ("_endDate", SearchOperator.EndDate),
        ("_contains", SearchOperator.Contains),
        ("_exact", SearchOperator.Exact),
        ("_lte", SearchOperator.LessThanOrEqual),
        ("_gte", SearchOperator.GreaterThanOrEqual),
        ("_lt", SearchOperator.LessThan),
        ("_gt", SearchOperator.GreaterThan),
    ];

    /// <summary>
    /// Determines whether the query string contains any non-ignored dynamic search parameters.
    /// </summary>
    /// <param name="parameters">The query-string parameters to inspect.</param>
    /// <param name="options">Optional parser settings for ignored keys and prefixes.</param>
    /// <returns><see langword="true" /> when at least one non-empty dynamic search parameter is present.</returns>
    public static bool HasDynamicSearchParameters(
        IQueryCollection parameters,
        DynamicSearchQueryParserOptions? options = null)
    {
        return parameters.Any(parameter => IsDynamicSearchParameter(parameter, options));
    }

    /// <summary>
    /// Parses query-string parameters into validated dynamic search filters.
    /// </summary>
    /// <param name="parameters">The query-string parameters to parse.</param>
    /// <param name="fields">The searchable field definitions used for validation.</param>
    /// <param name="options">Optional parser settings for ignored keys and prefixes.</param>
    /// <returns>A parse result containing valid filters and validation errors.</returns>
    public static DynamicSearchFilterParseResult Parse(
        IQueryCollection parameters,
        IEnumerable<DynamicSearchField> fields,
        DynamicSearchQueryParserOptions? options = null)
    {
        Dictionary<string, DynamicSearchField> fieldsByName = fields.ToDictionary(
            field => field.Name,
            StringComparer.OrdinalIgnoreCase);

        List<DynamicSearchFilter> filters = [];
        List<DynamicSearchParseError> errors = [];

        foreach (KeyValuePair<string, StringValues> parameter in parameters)
        {
            if (!IsDynamicSearchParameter(parameter, options))
            {
                continue;
            }

            string value = parameter.Value.FirstOrDefault()?.Trim() ?? string.Empty;

            if (!TryParseDynamicFilterKey(parameter.Key, out string fieldName, out SearchOperator searchOperator))
            {
                errors.Add(new DynamicSearchParseError(
                    DynamicSearchParseErrorCode.UnsupportedSearchParameter,
                    parameter.Key,
                    Value: value));
                continue;
            }

            if (!SafeDynamicFieldName.IsMatch(fieldName))
            {
                errors.Add(new DynamicSearchParseError(
                    DynamicSearchParseErrorCode.InvalidFieldName,
                    parameter.Key,
                    fieldName,
                    searchOperator,
                    value));
                continue;
            }

            if (!fieldsByName.TryGetValue(fieldName, out DynamicSearchField? field))
            {
                errors.Add(new DynamicSearchParseError(
                    DynamicSearchParseErrorCode.UnknownField,
                    parameter.Key,
                    fieldName,
                    searchOperator,
                    value));
                continue;
            }

            if (!ValidateDynamicFilter(parameter.Key, field, searchOperator, value, errors))
            {
                continue;
            }

            filters.Add(new DynamicSearchFilter(field.Name, field.FieldType, searchOperator, value));
        }

        return new DynamicSearchFilterParseResult(filters, errors);
    }

    private static bool IsDynamicSearchParameter(
        KeyValuePair<string, StringValues> parameter,
        DynamicSearchQueryParserOptions? options)
    {
        if (IsIgnoredKey(parameter.Key, options))
        {
            return false;
        }

        string? value = parameter.Value.FirstOrDefault()?.Trim();
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool IsIgnoredKey(string key, DynamicSearchQueryParserOptions? options)
    {
        if (options is null)
        {
            return false;
        }

        return options.IgnoredKeys.Contains(key)
            || options.IgnoredKeyPrefixes.Any(prefix => key.StartsWith(prefix, StringComparison.Ordinal));
    }

    private static bool ValidateDynamicFilter(
        string queryKey,
        DynamicSearchField field,
        SearchOperator searchOperator,
        string value,
        List<DynamicSearchParseError> errors)
    {
        bool isValidOperator = field.FieldType switch
        {
            DynamicSearchFieldType.Text => searchOperator is SearchOperator.Contains or SearchOperator.StartsWith or SearchOperator.Exact,
            DynamicSearchFieldType.Number => searchOperator is SearchOperator.LessThan or SearchOperator.LessThanOrEqual or SearchOperator.Exact or SearchOperator.GreaterThan or SearchOperator.GreaterThanOrEqual,
            DynamicSearchFieldType.Date => searchOperator is SearchOperator.StartDate or SearchOperator.EndDate,
            DynamicSearchFieldType.Boolean => searchOperator == SearchOperator.Exact,
            DynamicSearchFieldType.Select => searchOperator == SearchOperator.Exact,
            _ => false,
        };

        if (!isValidOperator)
        {
            errors.Add(new DynamicSearchParseError(
                DynamicSearchParseErrorCode.InvalidOperatorForFieldType,
                queryKey,
                field.Name,
                searchOperator,
                value));
            return false;
        }

        if (field.FieldType == DynamicSearchFieldType.Number &&
            !decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
        {
            errors.Add(new DynamicSearchParseError(
                DynamicSearchParseErrorCode.InvalidNumberValue,
                queryKey,
                field.Name,
                searchOperator,
                value));
            return false;
        }

        if (field.FieldType == DynamicSearchFieldType.Date &&
            !DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            errors.Add(new DynamicSearchParseError(
                DynamicSearchParseErrorCode.InvalidDateValue,
                queryKey,
                field.Name,
                searchOperator,
                value));
            return false;
        }

        if (field.FieldType == DynamicSearchFieldType.Boolean &&
            !bool.TryParse(value, out _))
        {
            errors.Add(new DynamicSearchParseError(
                DynamicSearchParseErrorCode.InvalidBooleanValue,
                queryKey,
                field.Name,
                searchOperator,
                value));
            return false;
        }

        if (field.FieldType == DynamicSearchFieldType.Select &&
            !field.Options.Any(option => option.Equals(value, StringComparison.Ordinal)))
        {
            errors.Add(new DynamicSearchParseError(
                DynamicSearchParseErrorCode.InvalidSelectOptionValue,
                queryKey,
                field.Name,
                searchOperator,
                value));
            return false;
        }

        return true;
    }

    private static bool TryParseDynamicFilterKey(
        string key,
        out string fieldName,
        out SearchOperator searchOperator)
    {
        foreach ((string suffix, SearchOperator filterOperator) in Suffixes)
        {
            if (key.EndsWith(suffix, StringComparison.Ordinal))
            {
                fieldName = key[..^suffix.Length];
                searchOperator = filterOperator;
                return !string.IsNullOrWhiteSpace(fieldName);
            }
        }

        fieldName = key;
        searchOperator = SearchOperator.Exact;
        return !string.IsNullOrWhiteSpace(fieldName);
    }
}
