using System.Globalization;
using System.Linq;
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
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DiscogsService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    /// <summary>
    /// Выполняет поиск мастер-релизов в Discogs.
    /// </summary>
    /// <param name="query">Поисковый запрос.</param>
    /// <param name="page">Номер страницы (начиная с 1).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список найденных мастер-релизов.</returns>
    public async Task<DiscogsSearchResponse> SearchMastersAsync(string query, int page = 1, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new DiscogsSearchResponse();
        }

        if (page <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Страница должна быть больше 0.");
        }

        var client = _httpClientFactory.CreateClient(DiscogsHttpClientName);

        var queryParameters = new Dictionary<string, string?>
        {
            ["type"] = "master",
            ["q"] = query.Trim(),
            ["page"] = page.ToString(CultureInfo.InvariantCulture),
            ["per_page"] = PageSize.ToString(CultureInfo.InvariantCulture)
        };

        var requestUri = BuildRequestUri(SearchEndpoint, queryParameters);

        using var response = await client.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var searchResponse = await JsonSerializer.DeserializeAsync<DiscogsSearchResponse>(contentStream, _serializerOptions, cancellationToken);

        return searchResponse ?? new DiscogsSearchResponse();
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

        var client = _httpClientFactory.CreateClient(DiscogsHttpClientName);

        using var response = await client.GetAsync(resourceUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var master = await JsonSerializer.DeserializeAsync<DiscogsMasterDetail>(contentStream, _serializerOptions, cancellationToken);

        if (master?.Tracklist.Count > 0)
        {
            var filteredTracklist = master.Tracklist
                .Where(track => string.Equals(track.Type, "track", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (filteredTracklist.Count != master.Tracklist.Count)
            {
                master = master with { Tracklist = filteredTracklist };
            }
        }

        return master;
    }

    /// <summary>
    /// Получает список версий указанного мастер-релиза.
    /// </summary>
    /// <param name="masterId">Идентификатор мастера.</param>
    /// <param name="page">Номер страницы (начиная с 1).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Ответ Discogs с пагинацией и сокращёнными данными по версиям.</returns>
    public async Task<DiscogsMasterVersionsResponse> GetMasterVersionsAsync(int masterId, int page = 1, CancellationToken cancellationToken = default)
    {
        if (masterId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(masterId), "Идентификатор мастера должен быть больше 0.");
        }

        if (page <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Страница должна быть больше 0.");
        }

        var client = _httpClientFactory.CreateClient(DiscogsHttpClientName);

        var queryParameters = new Dictionary<string, string?>
        {
            ["page"] = page.ToString(CultureInfo.InvariantCulture),
            ["per_page"] = PageSize.ToString(CultureInfo.InvariantCulture)
        };

        var path = $"masters/{masterId}/versions";
        var requestUri = BuildRequestUri(path, queryParameters);

        using var response = await client.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var versionsResponse = await JsonSerializer.DeserializeAsync<DiscogsMasterVersionsResponse>(contentStream, _serializerOptions, cancellationToken);

        return versionsResponse ?? new DiscogsMasterVersionsResponse();
    }

    /// <summary>
    /// Получает информацию о релизе по его идентификатору.
    /// </summary>
    /// <param name="releaseId">Идентификатор релиза.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<DiscogsReleaseDetail?> GetReleaseAsync(int releaseId, CancellationToken cancellationToken = default)
    {
        if (releaseId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(releaseId), "Идентификатор релиза должен быть больше 0.");
        }

        var client = _httpClientFactory.CreateClient(DiscogsHttpClientName);

        var path = $"releases/{releaseId}";

        using var response = await client.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<DiscogsReleaseDetail>(contentStream, _serializerOptions, cancellationToken);
    }

    /// <summary>
    /// Скачивает изображение по указанному URL через Discogs HttpClient.
    /// </summary>
    /// <param name="imageUrl">URL изображения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<byte[]?> DownloadImageAsync(string? imageUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        var client = _httpClientFactory.CreateClient(DiscogsHttpClientName);

        try
        {
            using var response = await client.GetAsync(imageUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
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

}

