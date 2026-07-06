using Dynamic.Json.EfCore.Search;

namespace Dynamic.Json.EfCore.AspNetCore;

public sealed record DynamicSearchFilterParseResult(
    IReadOnlyList<DynamicSearchFilter> Filters,
    IReadOnlyList<string> Errors);
