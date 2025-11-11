using DMonoStereo.Core.Models;

namespace DMonoStereo.ViewModels;

public class TrackListItemViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ArtistName { get; init; } = string.Empty;
    public string AlbumName { get; init; } = string.Empty;
    public string DurationText { get; init; } = string.Empty;
    public int? Rating { get; init; }
    public int? TrackNumber { get; init; }
    public int AlbumId { get; init; }
    public int ArtistId { get; init; }

    public string RatingText => Rating.HasValue ? $"★ {Rating}" : "★ —";

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