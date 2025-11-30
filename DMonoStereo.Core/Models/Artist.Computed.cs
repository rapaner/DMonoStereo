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

    public int RatedAlbumsCount => Albums?.Count(album => album.Rating.HasValue) ?? 0;

    public double RatedAlbumsPercentage
    {
        get
        {
            var totalAlbums = AlbumCount;
            if (totalAlbums == 0)
            {
                return 0;
            }

            return (double)RatedAlbumsCount / totalAlbums * 100;
        }
    }

    public int RatedTracksCount
    {
        get
        {
            return Albums?
                .SelectMany(album => album.Tracks ?? Enumerable.Empty<Track>())
                .Count(track => track.Rating.HasValue) ?? 0;
        }
    }

    public double RatedTracksPercentage
    {
        get
        {
            var totalTracks = TrackCount;
            if (totalTracks == 0)
            {
                return 0;
            }

            return (double)RatedTracksCount / totalTracks * 100;
        }
    }
}

