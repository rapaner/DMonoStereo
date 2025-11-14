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
    /// <summary>
    /// Уникальный идентификатор версии альбома.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Название альбома с уточняющей информацией версии.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Формат выпуска (CD, LP, цифровой и т.п.).
    /// </summary>
    public string? Format { get; init; }

    /// <summary>
    /// Страна происхождения из релиза Discogs.
    /// </summary>
    public string? Country { get; init; }

    /// <summary>
    /// Ссылка на детальные данные релиза в Discogs.
    /// </summary>
    public string? ResourceUrl { get; init; }

    /// <summary>
    /// Байты обложки, загруженные по ссылке thumb.
    /// </summary>
    public byte[]? CoverImageData { get; init; }
}