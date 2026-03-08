using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DMonoStereo.Core.Data;

/// <summary>
/// Переопределяет встроенные SQLite collation NOCASE и функцию like() на Unicode-aware реализации,
/// обеспечивая регистронезависимое сравнение и LIKE для нелатинских символов (кириллица и др.).
/// </summary>
public class SqliteUnicodeCollationInterceptor : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        RegisterUnicodeFunctions(connection);
    }

    public override Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        RegisterUnicodeFunctions(connection);
        return Task.CompletedTask;
    }

    public static void RegisterUnicodeFunctions(DbConnection connection)
    {
        if (connection is not SqliteConnection sqliteConnection)
            return;

        sqliteConnection.CreateCollation("NOCASE", (x, y) =>
            string.Compare(x, y, StringComparison.OrdinalIgnoreCase));

        sqliteConnection.CreateFunction("like", (string? pattern, string? input) =>
            SqliteLike(pattern, input, null));

        sqliteConnection.CreateFunction("like", (string? pattern, string? input, string? escape) =>
            SqliteLike(pattern, input, escape));
    }

    private static bool SqliteLike(string? pattern, string? input, string? escape)
    {
        if (pattern == null || input == null)
            return false;

        char? escapeChar = !string.IsNullOrEmpty(escape) ? escape[0] : null;

        int pi = 0, ii = 0;
        int patLen = pattern.Length, inputLen = input.Length;
        int percentPi = -1, percentIi = -1;

        while (ii < inputLen)
        {
            if (pi < patLen && escapeChar.HasValue && pattern[pi] == escapeChar.Value)
            {
                pi++;
                if (pi >= patLen || char.ToUpperInvariant(pattern[pi]) != char.ToUpperInvariant(input[ii]))
                    return false;
                pi++;
                ii++;
            }
            else if (pi < patLen && pattern[pi] == '%')
            {
                percentPi = pi;
                percentIi = ii;
                pi++;
            }
            else if (pi < patLen && (pattern[pi] == '_' ||
                     char.ToUpperInvariant(pattern[pi]) == char.ToUpperInvariant(input[ii])))
            {
                pi++;
                ii++;
            }
            else if (percentPi >= 0)
            {
                pi = percentPi + 1;
                ii = ++percentIi;
            }
            else
            {
                return false;
            }
        }

        while (pi < patLen && pattern[pi] == '%')
            pi++;

        return pi == patLen;
    }
}
