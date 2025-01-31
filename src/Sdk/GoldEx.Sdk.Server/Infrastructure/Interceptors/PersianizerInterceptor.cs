using System.Data.Common;
using GoldEx.Sdk.Common.Extensions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GoldEx.Sdk.Server.Infrastructure.Interceptors;

internal class PersianizerInterceptor : DbCommandInterceptor
{
    public override DbCommand CommandCreated(CommandEndEventData eventData, DbCommand result)
    {
        var cmd = base.CommandCreated(eventData, result);

        return cmd;
    }

    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        PersianizeParameters(command);

        return base.ScalarExecuting(command, eventData, result);
    }

    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        PersianizeParameters(command);

        return base.NonQueryExecuting(command, eventData, result);
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        PersianizeParameters(command);

        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        PersianizeParameters(command);

        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        PersianizeParameters(command);

        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
    {
        PersianizeParameters(command);

        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    private static void PersianizeParameters(DbCommand command)
    {
        foreach (DbParameter param in command.Parameters)
        {
            if (param.DbType == System.Data.DbType.String && param.Value is string)
            {
                var value = param.Value.ToString()!;

                param.Value = value.ToPersianChars()
                    .ToEnglishNumericsChars()
                    .Trim();
            }
        }
    }
}
