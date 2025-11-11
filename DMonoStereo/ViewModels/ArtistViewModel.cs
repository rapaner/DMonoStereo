using DMonoStereo.Core.Models;

namespace DMonoStereo.ViewModels;

public class ArtistViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int AlbumCount { get; init; }
    public int TrackCount { get; init; }
    public double AverageAlbumRating { get; init; }
    public byte[]? CoverImage { get; init; }
    public bool HasCoverImage { get; init; }

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