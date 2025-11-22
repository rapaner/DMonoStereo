using DMonoStereo.Models;
using DMonoStereo.Models.Discogs;
using System.Globalization;

namespace DMonoStereo.Services;

/// <summary>
/// Сервис высокого уровня для поисковых операций по музыке.
/// </summary>
public class MusicSearchService
{
    private readonly DiscogsService _discogsService;

    public MusicSearchService(DiscogsService discogsService)
    {
        _discogsService = discogsService ?? throw new ArgumentNullException(nameof(discogsService));
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

        var tasks = masters.Select(master => MapToResultAsync(master, cancellationToken)).ToArray();

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

        var coverImageUrl = master.Images.FirstOrDefault()?.Uri;
        var coverImageData = await _discogsService.DownloadImageAsync(coverImageUrl, cancellationToken);

        var artist = master.Artists.FirstOrDefault();
        var artistImageData = await _discogsService.DownloadImageAsync(artist?.ResourceUrl, cancellationToken);

        var tracks = MapAlbumTracks(master.Tracklist);

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

    /// <summary>
    /// Возвращает версии альбома с загруженными обложками.
    /// </summary>
    /// <param name="album">Результат поиска альбома.</param>
    /// <param name="page">Номер страницы (начиная с 1).</param>
    /// <param name="format">Фильтр по формату (необязательно).</param>
    /// <param name="country">Фильтр по стране (необязательно).</param>
    /// <param name="year">Фильтр по году выпуска (необязательно).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<MusicAlbumVersionsResponse> GetAlbumVersionsAsync(
        MusicAlbumSearchResult album,
        int page = 1,
        string? format = null,
        string? country = null,
        int? year = null,
        CancellationToken cancellationToken = default)
    {
        if (album is null)
        {
            throw new ArgumentNullException(nameof(album));
        }

        if (page <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Страница должна быть больше 0.");
        }

        if (album.MasterId is null or <= 0)
        {
            return new MusicAlbumVersionsResponse();
        }

        var discogsResponse = await _discogsService.GetMasterVersionsAsync(
            album.MasterId.Value,
            page,
            format,
            country,
            year,
            cancellationToken);

        var formatFilters = MapFilterFacet(discogsResponse.FilterFacets, "format");
        var countryFilters = MapFilterFacet(discogsResponse.FilterFacets, "country");
        var yearFilters = MapFilterFacet(discogsResponse.FilterFacets, "released");

        if (discogsResponse.Versions.Count == 0)
        {
            return new MusicAlbumVersionsResponse
            {
                Pagination = discogsResponse.Pagination,
                FormatFilters = formatFilters,
                CountryFilters = countryFilters,
                YearFilters = yearFilters
            };
        }

        var versions = new List<MusicAlbumVersionSummary>(discogsResponse.Versions.Count);

        foreach (var version in discogsResponse.Versions)
        {
            var coverImageData = await _discogsService.DownloadImageAsync(version.Thumb, cancellationToken);

            versions.Add(new MusicAlbumVersionSummary
            {
                Id = version.Id,
                Title = version.Title,
                Format = version.Format,
                Country = version.Country,
                ResourceUrl = version.ResourceUrl,
                CoverImageData = coverImageData
            });
        }

        return new MusicAlbumVersionsResponse
        {
            Versions = versions,
            Pagination = discogsResponse.Pagination,
            FormatFilters = formatFilters,
            CountryFilters = countryFilters,
            YearFilters = yearFilters
        };
    }

    /// <summary>
    /// Получает детальную информацию о конкретной версии альбома (релизе).
    /// </summary>
    /// <param name="version">Данные версии альбома.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<MusicAlbumVersionDetail?> GetAlbumVersionAsync(
        MusicAlbumVersionSummary version,
        CancellationToken cancellationToken = default)
    {
        if (version is null)
        {
            throw new ArgumentNullException(nameof(version));
        }

        if (version.Id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(version.Id), "Идентификатор версии должен быть больше 0.");
        }

        var release = await _discogsService.GetReleaseAsync(version.Id, cancellationToken);

        if (release is null)
        {
            return null;
        }

        var artist = release.Artists.FirstOrDefault();
        var image = release.Images.FirstOrDefault();

        var artistImageData = await _discogsService.DownloadImageAsync(artist?.ThumbnailUrl, cancellationToken);
        var coverImageData = await _discogsService.DownloadImageAsync(image?.ResourceUrl, cancellationToken);

        var tracks = MapReleaseTracks(release.Tracklist);

        return new MusicAlbumVersionDetail
        {
            Id = release.Id,
            Title = release.Title,
            Format = version.Format,
            Country = release.Country,
            Released = release.Released,
            Year = release.Year,
            Artist = artist is null
                ? null
                : new MusicAlbumVersionArtist
                {
                    Name = artist.Name,
                    ThumbnailImageData = artistImageData
                },
            Image = image is null
                ? null
                : new MusicAlbumVersionImage
                {
                    ImageData = coverImageData
                },
            Tracklist = tracks
        };
    }

    private static IReadOnlyList<MusicAlbumDetailTrack> MapAlbumTracks(
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

    private static IReadOnlyList<MusicAlbumVersionTrack> MapReleaseTracks(
        IReadOnlyList<DiscogsReleaseDetail.DiscogsReleaseTrack> tracklist)
    {
        if (tracklist is null || tracklist.Count == 0)
        {
            return Array.Empty<MusicAlbumVersionTrack>();
        }

        var result = new List<MusicAlbumVersionTrack>(tracklist.Count);
        var position = 1;

        foreach (var track in tracklist)
        {
            result.Add(new MusicAlbumVersionTrack
            {
                Position = position++,
                Title = track.Title,
                Duration = track.Duration
            });
        }

        return result;
    }

    private async Task<MusicAlbumSearchResult> MapToResultAsync(
        DiscogsMasterSummary master,
        CancellationToken cancellationToken)
    {
        var coverImageData = await _discogsService.DownloadImageAsync(master.CoverImage, cancellationToken);
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

    private static IReadOnlyList<MusicFilterOption> MapFilterFacet(
        IReadOnlyList<DiscogsFilterFacet> filterFacets,
        string facetId)
    {
        if (filterFacets is null || filterFacets.Count == 0)
        {
            return Array.Empty<MusicFilterOption>();
        }

        var facet = filterFacets.FirstOrDefault(f => string.Equals(f.Id, facetId, StringComparison.OrdinalIgnoreCase));
        if (facet is null || facet.Values.Count == 0)
        {
            return Array.Empty<MusicFilterOption>();
        }

        return facet.Values
            .Select(v => new MusicFilterOption
            {
                Title = v.Title,
                Value = v.Value,
                Count = v.Count
            })
            .ToList();
    }
}