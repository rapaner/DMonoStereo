namespace DMonoStereo.Models;

/// <summary>
/// Облегчённая проекция исполнителя с агрегатами для списочного отображения.
/// </summary>
public record ArtistSummary(
    int Id,
    string Name,
    byte[]? CoverImage,
    int AlbumCount,
    int TrackCount,
    double? AverageTrackRating,
    int RatedTracksCount);
