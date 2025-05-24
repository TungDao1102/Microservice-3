using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace BuildingBlocks.Common.Interceptors
{
    public class DatabaseInterceptor : DbCommandInterceptor
    {
        public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            if (IsDangerousSql(command.CommandText))
                throw new InvalidOperationException("Dangerous SQL detected: Missing WHERE in query" + command.CommandText);
            return base.NonQueryExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (IsDangerousSql(command.CommandText))
                throw new InvalidOperationException("Dangerous SQL detected: Missing WHERE in query" + command.CommandText);
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }

        private static bool IsDangerousSql(string sql)
        {
            // Check for dangerous SQL patterns
            bool isDeleteOrUpdate = sql.Contains("UPDATE", StringComparison.OrdinalIgnoreCase) ||
                   sql.Contains("DELETE", StringComparison.OrdinalIgnoreCase);
            bool hasNoWhere = !sql.Contains("WHERE", StringComparison.OrdinalIgnoreCase);
            return isDeleteOrUpdate && hasNoWhere;
        }
    }
}
