using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DMonoStereo.Core.Data;

/// <summary>
/// Переопределяет встроенную SQLite collation NOCASE на Unicode-aware реализацию,
/// обеспечивая регистронезависимое сравнение для нелатинских символов (кириллица и др.).
/// </summary>
public class SqliteUnicodeCollationInterceptor : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        RegisterCollation(connection);
    }

    public override Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        RegisterCollation(connection);
        return Task.CompletedTask;
    }

    private static void RegisterCollation(DbConnection connection)
    {
        if (connection is SqliteConnection sqliteConnection)
        {
            sqliteConnection.CreateCollation("NOCASE", (x, y) =>
                string.Compare(x, y, StringComparison.OrdinalIgnoreCase));
        }
    }
}
