namespace DMonoStereo.Models;

/// <summary>
/// Представляет результаты поиска музыкального альбома.
/// </summary>
public record MusicAlbumSearchResult
{
    /// <summary>
    /// Название альбома.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Ссылка на ресурс Discogs.
    /// </summary>
    public string? ResourceUrl { get; init; }

    /// <summary>
    /// Данные изображения обложки.
    /// </summary>
    public byte[]? CoverImageData { get; init; }

    /// <summary>
    /// Год выпуска.
    /// </summary>
    public int? Year { get; init; }

    /// <summary>
    /// Идентификатор результата поиска.
    /// </summary>
    public int? Id { get; init; }

    /// <summary>
    /// Идентификатор мастер-релиза.
    /// </summary>
    public int? MasterId { get; init; }
}


