using System.Collections.Generic;
using DMonoStereo.Models.Discogs;

namespace DMonoStereo.Models;

/// <summary>
/// Ответ сервиса поиска по версиям альбома.
/// </summary>
public record MusicAlbumVersionsResponse
{
    /// <summary>
    /// Список версий.
    /// </summary>
    public List<MusicAlbumVersionSummary> Versions { get; init; } = [];

    /// <summary>
    /// Пагинация, полученная из Discogs.
    /// </summary>
    public DiscogsPagination? Pagination { get; init; }
}

/// <summary>
/// Краткие данные о версии альбома с обложкой.
/// </summary>
public record MusicAlbumVersionSummary
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public string? Format { get; init; }
    public string? ResourceUrl { get; init; }

    /// <summary>
    /// Байты обложки, загруженные по ссылке thumb.
    /// </summary>
    public byte[]? CoverImageData { get; init; }
}


