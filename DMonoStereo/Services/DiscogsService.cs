using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using DMonoStereo.Models;
using DMonoStereo.Models.Discogs;

namespace DMonoStereo.Services;

/// <summary>
/// Сервис для взаимодействия с Discogs API.
/// </summary>
public class DiscogsService
{
    private const string DiscogsHttpClientName = "DiscogsClient";
    private const string SearchEndpoint = "database/search";
    private const int PageSize = 10;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AppConfiguration _appConfiguration;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DiscogsService(IHttpClientFactory httpClientFactory, AppConfiguration appConfiguration)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _appConfiguration = appConfiguration ?? throw new ArgumentNullException(nameof(appConfiguration));
    }

    /// <summary>
    /// Выполняет поиск мастер-релизов в Discogs.
    /// </summary>
    /// <param name="query">Поисковый запрос.</param>
    /// <param name="page">Номер страницы (начиная с 1).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список найденных мастер-релизов.</returns>
    public async Task<IReadOnlyList<DiscogsMasterSummary>> SearchMastersAsync(string query, int page = 1, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<DiscogsMasterSummary>();
        }

        if (page <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Страница должна быть больше 0.");
        }

        var (key, secret) = GetDiscogsCredentials();

        var client = _httpClientFactory.CreateClient(DiscogsHttpClientName);

        var queryParameters = new Dictionary<string, string?>
        {
            ["type"] = "master",
            ["q"] = query.Trim(),
            ["page"] = page.ToString(CultureInfo.InvariantCulture),
            ["per_page"] = PageSize.ToString(CultureInfo.InvariantCulture),
            ["key"] = key,
            ["secret"] = secret
        };

        var requestUri = BuildRequestUri(SearchEndpoint, queryParameters);

        using var response = await client.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var searchResponse = await JsonSerializer.DeserializeAsync<DiscogsSearchResponse>(contentStream, _serializerOptions, cancellationToken);

        return searchResponse?.Results ?? new List<DiscogsMasterSummary>();
    }

    /// <summary>
    /// Получает информацию о мастер-релизе по URL.
    /// </summary>
    /// <param name="resourceUrl">Полный URL мастер-релиза.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<DiscogsMasterDetail?> GetMasterAsync(string resourceUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resourceUrl))
        {
            throw new ArgumentException("URL мастер-релиза не может быть пустым.", nameof(resourceUrl));
        }

        if (!Uri.TryCreate(resourceUrl, UriKind.Absolute, out var _))
        {
            throw new ArgumentException("Некорректный URL мастер-релиза.", nameof(resourceUrl));
        }

        var (key, secret) = GetDiscogsCredentials();
        var requestUri = AppendAuthParameters(resourceUrl, key, secret);
        var client = _httpClientFactory.CreateClient(DiscogsHttpClientName);

        using var response = await client.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<DiscogsMasterDetail>(contentStream, _serializerOptions, cancellationToken);
    }

    private (string key, string secret) GetDiscogsCredentials()
    {
        var key = _appConfiguration.DiscogsKey;
        var secret = _appConfiguration.DiscogsSecret;

        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("Конфигурация Discogs API не настроена.");
        }

        return (key, secret);
    }

    private static string AppendAuthParameters(string resourceUrl, string key, string secret)
    {
        var uri = new Uri(resourceUrl, UriKind.Absolute);
        var parameters = ParseQueryParameters(uri.Query);

        parameters["key"] = key;
        parameters["secret"] = secret;

        return BuildRequestUri(uri.GetLeftPart(UriPartial.Path), parameters);
    }

    private static string BuildRequestUri(string path, IDictionary<string, string?> parameters)
    {
        var builder = new StringBuilder(path);
        builder.Append('?');
        var first = true;

        foreach (var parameter in parameters)
        {
            if (string.IsNullOrEmpty(parameter.Value))
            {
                continue;
            }

            if (!first)
            {
                builder.Append('&');
            }

            builder
                .Append(Uri.EscapeDataString(parameter.Key))
                .Append('=')
                .Append(Uri.EscapeDataString(parameter.Value));

            first = false;
        }

        return builder.ToString();
    }

    private static Dictionary<string, string?> ParseQueryParameters(string query)
    {
        var result = new Dictionary<string, string?>();

        if (string.IsNullOrEmpty(query))
        {
            return result;
        }

        var trimmedQuery = query.TrimStart('?');

        if (trimmedQuery.Length == 0)
        {
            return result;
        }

        var pairs = trimmedQuery.Split('&', StringSplitOptions.RemoveEmptyEntries);

        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=', 2);
            if (keyValue.Length == 0 || string.IsNullOrEmpty(keyValue[0]))
            {
                continue;
            }

            var name = Uri.UnescapeDataString(keyValue[0]);
            string? value = null;

            if (keyValue.Length > 1)
            {
                value = Uri.UnescapeDataString(keyValue[1]);
            }

            result[name] = value;
        }

        return result;
    }
}

