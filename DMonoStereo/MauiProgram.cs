using DMonoStereo.Converters;
using DMonoStereo.Core.Data;
using DMonoStereo.Models;
using DMonoStereo.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Reflection;

namespace DMonoStereo;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        ConfigureConfiguration(builder);
        ConfigureServices(builder);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void ConfigureConfiguration(MauiAppBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var configBuilder = new ConfigurationBuilder();

        using var baseStream = assembly.GetManifestResourceStream("DMonoStereo.appsettings.json");
        if (baseStream != null)
        {
            configBuilder.AddJsonStream(baseStream);
        }

        using var developmentStream = assembly.GetManifestResourceStream("DMonoStereo.appsettings.Development.json");
        if (developmentStream != null)
        {
            configBuilder.AddJsonStream(developmentStream);
        }

        using var releaseStream = assembly.GetManifestResourceStream("DMonoStereo.appsettings.Release.json");
        if (releaseStream != null)
        {
            configBuilder.AddJsonStream(releaseStream);
        }

        configBuilder.AddEnvironmentVariables();

        builder.Configuration.AddConfiguration(configBuilder.Build());
    }

    private static void ConfigureServices(MauiAppBuilder builder)
    {
        var appConfiguration = new AppConfiguration
        {
            AppDataDirectory = FileSystem.AppDataDirectory,
            DatabaseFileName = "dmonostereo.db",
            DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "dmonostereo.db"),
            AppVersion = AppInfo.VersionString,
            AppName = AppInfo.Name,
            YandexOAuthClientId = builder.Configuration.GetValue<string>("YandexOAuthClientId") ?? string.Empty,
            DiscogsKey = builder.Configuration.GetValue<string>("Discogs:Key") ?? string.Empty,
            DiscogsSecret = builder.Configuration.GetValue<string>("Discogs:Secret") ?? string.Empty,
            DiscogsToken = builder.Configuration.GetValue<string>("Discogs:Token") ?? string.Empty
        };

        builder.Services.AddSingleton(appConfiguration);
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddSingleton<YandexDiskService>();
        builder.Services.AddSingleton<YandexOAuthService>();
        builder.Services.AddSingleton<ImageService>();
        builder.Services.AddSingleton<ByteArrayToImageSourceConverter>();

        var sqliteConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = appConfiguration.DatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
            Pooling = false,
            DefaultTimeout = 60
        }.ToString();

        builder.Services.AddDbContext<MusicDbContext>(options =>
        {
            options.UseSqlite(sqliteConnectionString, sqliteOptions =>
            {
                sqliteOptions.MigrationsAssembly(typeof(MusicDbContext).Assembly.GetName().Name);
            });
        });

        builder.Services.AddDbContextFactory<MusicDbContext>(options =>
        {
            options.UseSqlite(sqliteConnectionString, sqliteOptions =>
            {
                sqliteOptions.MigrationsAssembly(typeof(MusicDbContext).Assembly.GetName().Name);
            });
        });

        builder.Services.AddHttpClient("DiscogsClient", (sp, client) =>
        {
            var configuration = sp.GetRequiredService<AppConfiguration>();

            if (string.IsNullOrWhiteSpace(configuration.DiscogsToken))
            {
                throw new InvalidOperationException("Конфигурация Discogs token не настроена.");
            }

            client.BaseAddress = new Uri("https://api.discogs.com/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("DMonoStereo/1.0");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Discogs", $"token={configuration.DiscogsToken}");
        });
        builder.Services.AddScoped<DatabaseMigrationService>();
        builder.Services.AddScoped<MusicService>();
        builder.Services.AddSingleton<DiscogsService>();
        builder.Services.AddTransient<MusicSearchService>();

        // Pages and Shell
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<Views.AddEditArtistPage>();
        builder.Services.AddTransient<Views.ArtistDetailPage>();
        builder.Services.AddTransient<Views.AddEditAlbumPage>();
        builder.Services.AddTransient<Views.AlbumDetailPage>();
        builder.Services.AddTransient<Views.AddEditTrackPage>();
        builder.Services.AddTransient<Views.AllArtistsPage>();
        builder.Services.AddTransient<Views.AllAlbumsPage>();
        builder.Services.AddTransient<Views.AllTracksPage>();
        builder.Services.AddTransient<Views.SettingsPage>();
        builder.Services.AddTransient<Views.YandexDiskPage>();
        builder.Services.AddTransient<Views.AlbumSearchPage>();
        builder.Services.AddTransient<Views.AddAlbumFromSearchPage>();
    }
}