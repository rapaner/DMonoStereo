using DMonoStereo.Core.Models;
using DMonoStereo.Helpers;

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
    public double? Rating { get; init; }

    /// <summary>
    /// Признак наличия рейтинга.
    /// </summary>
    public bool HasRating => Rating.HasValue;

    /// <summary>
    /// Средний рейтинг оценённых треков альбома.
    /// </summary>
    public double? AverageTrackRating { get; init; }

    /// <summary>
    /// Признак наличия среднего рейтинга треков.
    /// </summary>
    public bool HasAverageTrackRating => AverageTrackRating.HasValue;

    /// <summary>
    /// Изображение обложки в бинарном виде.
    /// </summary>
    public byte[]? CoverImage { get; init; }

    /// <summary>
    /// Суммарная продолжительность альбома в текстовом виде.
    /// </summary>
    public string TotalDurationText { get; init; } = string.Empty;

    /// <summary>
    /// Признак наличия суммарной продолжительности.
    /// </summary>
    public bool HasTotalDuration => !string.IsNullOrEmpty(TotalDurationText);

    /// <summary>
    /// Количество оцененных треков.
    /// </summary>
    public int RatedTracksCount { get; init; }

    /// <summary>
    /// Процент оцененных треков.
    /// </summary>
    public double RatedTracksPercentage { get; init; }

    /// <summary>
    /// Текст с количеством и процентом оцененных треков.
    /// </summary>
    public string RatedTracksText => $"Оценено: {RatedTracksCount} ({RatedTracksPercentage:F2}%)";

    /// <summary>
    /// Создаёт ViewModel на основе доменной модели альбома.
    /// </summary>
    /// <param name="album">Доменная модель альбома.</param>
    /// <returns>Инициализированный экземпляр ViewModel.</returns>
    public static AlbumViewModel FromAlbum(Album album)
    {
        var rating = album.Rating ?? null;
        var trackCount = album.Tracks.Count;

        string totalDurationText = string.Empty;
        if (album.TotalDuration.HasValue)
        {
            totalDurationText = TimeSpanHelpers.FormatDuration(album.TotalDuration.Value);
        }

        return new AlbumViewModel
        {
            Id = album.Id,
            Name = album.Name,
            ArtistName = album.Artist?.Name ?? string.Empty,
            Year = album.Year,
            TrackCount = trackCount,
            Rating = rating,
            AverageTrackRating = album.AverageTrackRating,
            CoverImage = album.CoverImage,
            TotalDurationText = totalDurationText,
            RatedTracksCount = album.RatedTracksCount,
            RatedTracksPercentage = album.RatedTracksPercentage
        };
    }
}