namespace DMonoStereo.Models;

/// <summary>
/// Детальная информация об альбоме, полученная из Discogs.
/// </summary>
public record MusicAlbumDetail
{
    /// <summary>
    /// Название альбома.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Год выпуска.
    /// </summary>
    public int? Year { get; init; }

    /// <summary>
    /// Имя основного артиста.
    /// </summary>
    public string? ArtistName { get; init; }

    /// <summary>
    /// Данные изображения обложки.
    /// </summary>
    public byte[]? CoverImageData { get; init; }

    /// <summary>
    /// Данные изображения артиста.
    /// </summary>
    public byte[]? ArtistImageData { get; init; }

    /// <summary>
    /// Треки альбома.
    /// </summary>
    public IReadOnlyList<MusicAlbumDetailTrack> Tracks { get; init; } = new List<MusicAlbumDetailTrack>();
}

/// <summary>
/// Информация о треке в альбоме.
/// </summary>
public record MusicAlbumDetailTrack
{
    /// <summary>
    /// Позиция трека (числовая).
    /// </summary>
    public int Position { get; init; }

    /// <summary>
    /// Название трека.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Длительность трека.
    /// </summary>
    public string? Duration { get; init; }
}