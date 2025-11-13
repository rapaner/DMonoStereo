using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Linq;
using DMonoStereo.Models;
using DMonoStereo.Models.Discogs;

namespace DMonoStereo.Services;

/// <summary>
/// Сервис высокого уровня для поисковых операций по музыке.
/// </summary>
public class MusicSearchService
{
    private const string DiscogsHttpClientName = "DiscogsClient";

    private readonly DiscogsService _discogsService;
    private readonly IHttpClientFactory _httpClientFactory;

    public MusicSearchService(DiscogsService discogsService, IHttpClientFactory httpClientFactory)
    {
        _discogsService = discogsService ?? throw new ArgumentNullException(nameof(discogsService));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    /// <summary>
    /// Выполняет поиск альбомов, обогащая результаты изображениями обложек.
    /// </summary>
    /// <param name="query">Поисковый запрос.</param>
    /// <param name="page">Номер страницы.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Коллекция результатов поиска альбомов.</returns>
    public async Task<MusicAlbumSearchResponse> SearchAlbumsAsync(
        string query,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var discogsResponse = await _discogsService.SearchMastersAsync(query, page, cancellationToken);
        var masters = discogsResponse.Results;
        var totalPages = discogsResponse.Pagination?.Pages ?? 0;

        if (masters.Count == 0)
        {
            return new MusicAlbumSearchResponse
            {
                TotalPages = totalPages
            };
        }

        var client = _httpClientFactory.CreateClient(DiscogsHttpClientName);
        var tasks = masters.Select(master => MapToResultAsync(master, client, cancellationToken)).ToArray();

        var results = await Task.WhenAll(tasks);

        return new MusicAlbumSearchResponse
        {
            Results = results,
            TotalPages = totalPages
        };
    }

    /// <summary>
    /// Получает детальную информацию об альбоме на основе результата поиска.
    /// </summary>
    /// <param name="album">Результат поиска альбома.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Обогащённый объект с деталями альбома или null, если данные недоступны.</returns>
    public async Task<MusicAlbumDetail?> GetAlbumAsync(
        MusicAlbumSearchResult album,
        CancellationToken cancellationToken = default)
    {
        if (album is null)
        {
            throw new ArgumentNullException(nameof(album));
        }

        if (string.IsNullOrWhiteSpace(album.ResourceUrl))
        {
            return null;
        }

        var master = await _discogsService.GetMasterAsync(album.ResourceUrl, cancellationToken);

        if (master is null)
        {
            return null;
        }

        var client = _httpClientFactory.CreateClient(DiscogsHttpClientName);

        var coverImageUrl = master.Images.FirstOrDefault()?.Uri;
        var coverImageData = await DownloadImageAsync(client, coverImageUrl, cancellationToken);

        var artist = master.Artists.FirstOrDefault();
        var artistImageData = await DownloadImageAsync(client, artist?.ResourceUrl, cancellationToken);

        var tracks = MapTracks(master.Tracklist);

        return new MusicAlbumDetail
        {
            Title = master.Title,
            Year = master.Year,
            ArtistName = artist?.Name,
            CoverImageData = coverImageData,
            ArtistImageData = artistImageData,
            Tracks = tracks
        };
    }

    private static IReadOnlyList<MusicAlbumDetailTrack> MapTracks(
        IReadOnlyList<DiscogsMasterDetail.DiscogsMasterTrack> tracklist)
    {
        if (tracklist.Count == 0)
        {
            return Array.Empty<MusicAlbumDetailTrack>();
        }

        var result = new List<MusicAlbumDetailTrack>(tracklist.Count);
        var position = 1;

        foreach (var track in tracklist)
        {
            result.Add(new MusicAlbumDetailTrack
            {
                Position = position++,
                Title = track.Title,
                Duration = track.Duration
            });
        }

        return result;
    }

    private static async Task<MusicAlbumSearchResult> MapToResultAsync(
        DiscogsMasterSummary master,
        HttpClient httpClient,
        CancellationToken cancellationToken)
    {
        var coverImageData = await DownloadImageAsync(httpClient, master.CoverImage, cancellationToken);
        var year = ParseYear(master.Year);

        return new MusicAlbumSearchResult
        {
            Title = master.Title,
            ResourceUrl = master.ResourceUrl,
            Year = year,
            Id = master.Id,
            MasterId = master.MasterId,
            CoverImageData = coverImageData
        };
    }

    private static int? ParseYear(string? yearValue)
    {
        if (string.IsNullOrWhiteSpace(yearValue))
        {
            return null;
        }

        if (int.TryParse(yearValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var year) && year > 0)
        {
            return year;
        }

        return null;
    }

    private static async Task<byte[]?> DownloadImageAsync(
        HttpClient httpClient,
        string? imageUrl,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        try
        {
            using var response = await httpClient.GetAsync(imageUrl, cancellationToken);

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
}


