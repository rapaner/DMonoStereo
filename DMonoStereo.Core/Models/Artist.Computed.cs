using System.Linq;

namespace DMonoStereo.Core.Models;

public partial record Artist
{
    public int AlbumCount => Albums?.Count ?? 0;

    public int TrackCount => Albums?.Sum(album => album.Tracks?.Count ?? 0) ?? 0;

    public double? AverageAlbumRating
    {
        get
        {
            var ratings = Albums?
                .Where(album => album.Rating.HasValue)
                .Select(album => (double)album.Rating!.Value)
                .ToList();

            if (ratings == null || ratings.Count == 0)
            {
                return null;
            }

            return ratings.Average();
        }
    }

    public double? AverageTrackRating
    {
        get
        {
            var ratings = Albums?
                .SelectMany(album => album.Tracks ?? Enumerable.Empty<Track>())
                .Where(track => track.Rating.HasValue)
                .Select(track => (double)track.Rating!.Value)
                .ToList();

            if (ratings == null || ratings.Count == 0)
            {
                return null;
            }

            return ratings.Average();
        }
    }
}

