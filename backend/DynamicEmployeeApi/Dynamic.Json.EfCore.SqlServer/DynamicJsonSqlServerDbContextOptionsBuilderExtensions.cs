using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Dynamic.Json.EfCore.SqlServer;

public static class DynamicJsonSqlServerDbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseDynamicJsonSqlServer(this DbContextOptionsBuilder builder)
    {
        return builder.ReplaceService<IMethodCallTranslatorProvider, DynamicJsonSqlServerMethodCallTranslatorProvider>();
    }
}
