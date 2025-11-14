using DMonoStereo.Core.Models;

namespace DMonoStereo.ViewModels;

/// <summary>
/// ViewModel для отображения краткой информации об альбоме.
/// </summary>
public class AlbumViewModel
{
    /// <summary>
    /// Уникальный идентификатор альбома.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Название альбома.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Имя артиста альбома.
    /// </summary>
    public string ArtistName { get; init; } = string.Empty;

    /// <summary>
    /// Год выпуска.
    /// </summary>
    public int? Year { get; init; }

    /// <summary>
    /// Количество треков в альбоме.
    /// </summary>
    public int TrackCount { get; init; }

    /// <summary>
    /// Средняя пользовательская оценка.
    /// </summary>
    public double Rating { get; init; }

    /// <summary>
    /// Изображение обложки в бинарном виде.
    /// </summary>
    public byte[]? CoverImage { get; init; }

    /// <summary>
    /// Создаёт ViewModel на основе доменной модели альбома.
    /// </summary>
    /// <param name="album">Доменная модель альбома.</param>
    /// <returns>Инициализированный экземпляр ViewModel.</returns>
    public static AlbumViewModel FromAlbum(Album album)
    {
        var rating = album.Rating ?? 0;
        var trackCount = album.Tracks.Count;

        return new AlbumViewModel
        {
            Id = album.Id,
            Name = album.Name,
            ArtistName = album.Artist?.Name ?? string.Empty,
            Year = album.Year,
            TrackCount = trackCount,
            Rating = rating,
            CoverImage = album.CoverImage
        };
    }
}