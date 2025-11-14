using DMonoStereo.Core.Models;

namespace DMonoStereo.ViewModels;

/// <summary>
/// ViewModel для краткого отображения информации об артисте.
/// </summary>
public class ArtistViewModel
{
    /// <summary>
    /// Уникальный идентификатор артиста.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Имя артиста.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Количество альбомов у артиста.
    /// </summary>
    public int AlbumCount { get; init; }

    /// <summary>
    /// Количество треков у артиста.
    /// </summary>
    public int TrackCount { get; init; }

    /// <summary>
    /// Средний рейтинг альбомов.
    /// </summary>
    public double AverageAlbumRating { get; init; }

    /// <summary>
    /// Обложка артиста в бинарном виде.
    /// </summary>
    public byte[]? CoverImage { get; init; }

    /// <summary>
    /// Признак наличия изображения.
    /// </summary>
    public bool HasCoverImage { get; init; }

    /// <summary>
    /// Создаёт ViewModel на основе доменной модели артиста.
    /// </summary>
    /// <param name="artist">Доменная модель артиста.</param>
    /// <returns>Готовая ViewModel.</returns>
    public static ArtistViewModel FromArtist(Artist artist)
    {
        var albumCount = artist.Albums.Count;
        var trackCount = artist.Albums.Sum(a => a.Tracks.Count);
        var ratedAlbums = artist.Albums.Where(a => a.Rating.HasValue).ToList();
        var averageRating = ratedAlbums.Count > 0
            ? ratedAlbums.Average(a => a.Rating!.Value)
            : 0;

        return new ArtistViewModel
        {
            Id = artist.Id,
            Name = artist.Name,
            AlbumCount = albumCount,
            TrackCount = trackCount,
            AverageAlbumRating = Math.Round(averageRating, 1),
            CoverImage = artist.CoverImage,
            HasCoverImage = artist.CoverImage is { Length: > 0 }
        };
    }
}