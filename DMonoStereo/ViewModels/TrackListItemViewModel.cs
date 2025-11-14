using DMonoStereo.Core.Models;

namespace DMonoStereo.ViewModels;

/// <summary>
/// ViewModel элемента списка треков с контекстом альбома и артиста.
/// </summary>
public class TrackListItemViewModel
{
    /// <summary>
    /// Уникальный идентификатор трека.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Название трека.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Имя артиста трека.
    /// </summary>
    public string ArtistName { get; init; } = string.Empty;

    /// <summary>
    /// Название альбома, к которому относится трек.
    /// </summary>
    public string AlbumName { get; init; } = string.Empty;

    /// <summary>
    /// Длительность трека в текстовом формате.
    /// </summary>
    public string DurationText { get; init; } = string.Empty;

    /// <summary>
    /// Рейтинг трека.
    /// </summary>
    public int? Rating { get; init; }

    /// <summary>
    /// Номер трека в альбоме.
    /// </summary>
    public int? TrackNumber { get; init; }

    /// <summary>
    /// Идентификатор альбома.
    /// </summary>
    public int AlbumId { get; init; }

    /// <summary>
    /// Идентификатор артиста.
    /// </summary>
    public int ArtistId { get; init; }

    /// <summary>
    /// Текстовое представление рейтинга.
    /// </summary>
    public string RatingText => Rating.HasValue ? $"★ {Rating}" : "★ —";

    /// <summary>
    /// Создаёт ViewModel на основе доменной модели трека.
    /// </summary>
    /// <param name="track">Доменная модель трека.</param>
    /// <returns>Экземпляр ViewModel.</returns>
    public static TrackListItemViewModel FromTrack(Track track)
    {
        var duration = TimeSpan.FromSeconds(track.Duration);
        var durationText = duration.Hours > 0
            ? duration.ToString(@"h\:mm\:ss")
            : duration.ToString(@"mm\:ss");

        return new TrackListItemViewModel
        {
            Id = track.Id,
            Name = track.Name,
            ArtistName = track.Album?.Artist?.Name ?? string.Empty,
            AlbumName = track.Album?.Name ?? string.Empty,
            DurationText = durationText,
            Rating = track.Rating,
            TrackNumber = track.TrackNumber,
            AlbumId = track.AlbumId,
            ArtistId = track.Album?.Artist?.Id ?? track.Album?.ArtistId ?? 0
        };
    }
}