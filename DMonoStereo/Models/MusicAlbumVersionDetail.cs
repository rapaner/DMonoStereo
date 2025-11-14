using System.Collections.Generic;

namespace DMonoStereo.Models;

/// <summary>
/// Детальная информация о версии альбома (релизе).
/// </summary>
public record MusicAlbumVersionDetail
{
    public int? Id { get; init; }
    public string? Title { get; init; }
    public string? Format { get; init; }
    public string? Country { get; init; }
    public string? Released { get; init; }
    public int? Year { get; init; }
    public MusicAlbumVersionArtist? Artist { get; init; }
    public MusicAlbumVersionImage? Image { get; init; }
    public IReadOnlyList<MusicAlbumVersionTrack> Tracklist { get; init; } = new List<MusicAlbumVersionTrack>();
}

/// <summary>
/// Информация об артисте релиза.
/// </summary>
public record MusicAlbumVersionArtist
{
    public string? Name { get; init; }
    public byte[]? ThumbnailImageData { get; init; }
}

/// <summary>
/// Изображение релиза.
/// </summary>
public record MusicAlbumVersionImage
{
    public byte[]? ImageData { get; init; }
}

/// <summary>
/// Трек релиза.
/// </summary>
public record MusicAlbumVersionTrack
{
    public int Position { get; init; }
    public string? Title { get; init; }
    public string? Duration { get; init; }
}


