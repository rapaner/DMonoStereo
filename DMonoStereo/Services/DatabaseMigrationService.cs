using DMonoStereo.Core.Data;
using DMonoStereo.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DMonoStereo.Services;

/// <summary>
/// Сервис для управления миграциями базы данных
/// </summary>
public class DatabaseMigrationService
{
    private readonly MusicDbContext _dbContext;
    private readonly IDbContextFactory<MusicDbContext> _contextFactory;
    private readonly string _databasePath;

    public DatabaseMigrationService(MusicDbContext dbContext, IDbContextFactory<MusicDbContext> contextFactory, AppConfiguration appConfiguration)
    {
        _dbContext = dbContext;
        _contextFactory = contextFactory;
        _databasePath = appConfiguration.DatabasePath;
    }

    /// <summary>
    /// Проверяет, существует ли таблица истории миграций
    /// </summary>
    public async Task<bool> IsMigrationHistoryTableExistsAsync()
    {
        if (!File.Exists(_databasePath))
        {
            return false;
        }

        const string query = @"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='__EFMigrationsHistory'";

        await using var connection = new SqliteConnection($"Data Source={_databasePath}");
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = query;

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    /// <summary>
    /// Применить ожидающие миграции
    /// </summary>
    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.CloseConnectionAsync();
        SqliteConnection.ClearAllPools();

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        try
        {
            await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("PRAGMA busy_timeout=5000;", cancellationToken);
        }
        catch
        {
            // ignore pragma failures on unsupported platforms
        }

        var pending = await context.Database.GetPendingMigrationsAsync(cancellationToken);
        if (!pending.Any())
        {
            return;
        }

        await context.Database.MigrateAsync(cancellationToken);
    }
}
