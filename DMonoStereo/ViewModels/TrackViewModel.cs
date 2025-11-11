using DMonoStereo.Core.Models;

namespace DMonoStereo.ViewModels;

public class TrackViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DurationText { get; init; } = string.Empty;
    public int? Rating { get; init; }
    public int? TrackNumber { get; init; }

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