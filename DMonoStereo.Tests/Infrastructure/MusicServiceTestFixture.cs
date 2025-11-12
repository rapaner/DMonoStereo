using DMonoStereo.Core.Data;
using DMonoStereo.Models;
using DMonoStereo.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DMonoStereo.Tests.Infrastructure;

/// <summary>
/// Фабрика для создания экземпляров <see cref="MusicService"/> с подключением к SQLite in-memory.
/// </summary>
public sealed class MusicServiceTestFixture
{
    /// <summary>
    /// Создать тестовый scope с in-memory SQLite и настроенным сервисом.
    /// </summary>
    public async Task<MusicServiceTestScope> CreateScopeAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection("DataSource=:memory:;Mode=Memory;Cache=Shared");
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var options = new DbContextOptionsBuilder<MusicDbContext>()
            .UseSqlite(connection)
            .Options;

        var dbContext = new MusicDbContext(options);
        await dbContext.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

        var appConfiguration = new AppConfiguration();

        var contextFactory = new DelegatingDbContextFactory(options);
        var migrationService = new DatabaseMigrationService(dbContext, contextFactory, appConfiguration);

        return new MusicServiceTestScope(connection, dbContext, migrationService);
    }

    private sealed class DelegatingDbContextFactory : IDbContextFactory<MusicDbContext>
    {
        private readonly DbContextOptions<MusicDbContext> _options;

        public DelegatingDbContextFactory(DbContextOptions<MusicDbContext> options)
        {
            _options = options;
        }

        public MusicDbContext CreateDbContext()
        {
            return new MusicDbContext(_options);
        }

        public ValueTask<MusicDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(CreateDbContext());
        }
    }
}

/// <summary>
/// Область жизни для тестов <see cref="MusicService"/> с in-memory SQLite.
/// </summary>
public sealed class MusicServiceTestScope : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    internal MusicServiceTestScope(
        SqliteConnection connection,
        MusicDbContext dbContext,
        DatabaseMigrationService migrationService)
    {
        _connection = connection;
        DbContext = dbContext;
        Service = new MusicService(dbContext, migrationService);
    }

    /// <summary>
    /// Экземпляр сервиса для тестов.
    /// </summary>
    public MusicService Service { get; }

    /// <summary>
    /// Основной контекст базы данных, используемый сервисом.
    /// </summary>
    public MusicDbContext DbContext { get; }

    public async ValueTask DisposeAsync()
    {
        await DbContext.DisposeAsync().ConfigureAwait(false);
        await _connection.DisposeAsync().ConfigureAwait(false);
    }
}

