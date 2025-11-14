using DMonoStereo.Core.Models;

namespace DMonoStereo.ViewModels;

/// <summary>
/// ViewModel для отображения базовой информации о треке.
/// </summary>
public class TrackViewModel
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
    /// Длительность трека в текстовом виде.
    /// </summary>
    public string DurationText { get; init; } = string.Empty;

    /// <summary>
    /// Рейтинг трека, если задан.
    /// </summary>
    public int? Rating { get; init; }

    /// <summary>
    /// Порядковый номер трека в альбоме.
    /// </summary>
    public int? TrackNumber { get; init; }

    /// <summary>
    /// Создаёт ViewModel на основе доменной модели трека.
    /// </summary>
    /// <param name="track">Доменная модель трека.</param>
    /// <returns>Готовый экземпляр ViewModel.</returns>
    public static TrackViewModel FromTrack(Track track)
    {
        var duration = TimeSpan.FromSeconds(track.Duration);
        var durationText = duration.Hours > 0
            ? duration.ToString(@"h\:mm\:ss")
            : duration.ToString(@"mm\:ss");

        return new TrackViewModel
        {
            Id = track.Id,
            Name = track.Name,
            DurationText = durationText,
            Rating = track.Rating,
            TrackNumber = track.TrackNumber
        };
    }
}