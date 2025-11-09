using System.Linq;
using DMonoStereo.Core.Models;

namespace DMonoStereo.ViewModels;

public class AlbumViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int? Year { get; init; }
    public int TrackCount { get; init; }
    public double Rating { get; init; }
    public byte[]? CoverImage { get; init; }

    public static AlbumViewModel FromAlbum(Album album)
    {
        var rating = album.Rating ?? 0;
        var trackCount = album.Tracks.Count;

        return new AlbumViewModel
        {
            Id = album.Id,
            Name = album.Name,
            Year = album.Year,
            TrackCount = trackCount,
            Rating = rating,
            CoverImage = album.CoverImage
        };
    }
}
