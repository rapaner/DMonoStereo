using DMonoStereo.Core.Models;
using DMonoStereo.Tests.Infrastructure;
using FluentAssertions;

namespace DMonoStereo.Tests.Services;

public class MusicServiceSortingTests
{
    private readonly MusicServiceTestFixture _fixture = new();

    [Fact]
    public async Task GetArtistsPageAsync_Should_Return_CaseInsensitive_Order()
    {
        await using var scope = await _fixture.CreateScopeAsync();
        var now = DateTime.UtcNow;

        var artistNames = new[] { "beta", "Alpha", "charlie", "Bravo" };
        var artists = artistNames.Select((name, index) => new Artist
        {
            Name = name,
            DateAdded = now.AddMinutes(index * -1)
        }).ToArray();

        scope.DbContext.Artists.AddRange(artists);
        await scope.DbContext.SaveChangesAsync();

        var result = await scope.Service.GetArtistsPageAsync(pageIndex: 0, pageSize: artists.Length);

        var expectedOrder = artistNames.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToArray();
        result.Select(a => a.Name).Should().Equal(expectedOrder);
    }

    [Fact]
    public async Task GetAlbumsPageAsync_Should_Return_CaseInsensitive_Order()
    {
        await using var scope = await _fixture.CreateScopeAsync();
        var now = DateTime.UtcNow;

        var artist = new Artist
        {
            Name = "Test Artist",
            DateAdded = now
        };

        var albumNames = new[] { "omega", "Alpha", "gamma", "beta" };
        var albums = albumNames.Select((name, index) => new Album
        {
            Name = name,
            Artist = artist,
            DateAdded = now.AddMinutes(index * -1)
        }).ToArray();

        scope.DbContext.Artists.Add(artist);
        scope.DbContext.Albums.AddRange(albums);
        await scope.DbContext.SaveChangesAsync();

        var result = await scope.Service.GetAlbumsPageAsync(pageIndex: 0, pageSize: albums.Length);

        var expectedOrder = albumNames.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToArray();
        result.Select(a => a.Name).Should().Equal(expectedOrder);
    }

    [Fact]
    public async Task GetTracksByAlbumAsync_Should_Order_By_Number_Then_Name_CaseInsensitive()
    {
        await using var scope = await _fixture.CreateScopeAsync();
        var now = DateTime.UtcNow;

        var artist = new Artist
        {
            Name = "Artist",
            DateAdded = now
        };

        var album = new Album
        {
            Name = "Album",
            Artist = artist,
            DateAdded = now
        };

        var tracks = new[]
        {
            new Track { Name = "gamma", TrackNumber = 2, Duration = 180, Album = album },
            new Track { Name = "Beta", TrackNumber = 1, Duration = 200, Album = album },
            new Track { Name = "alpha", TrackNumber = 1, Duration = 210, Album = album },
            new Track { Name = "delta", TrackNumber = null, Duration = 220, Album = album }
        };

        scope.DbContext.Artists.Add(artist);
        scope.DbContext.Albums.Add(album);
        scope.DbContext.Tracks.AddRange(tracks);
        await scope.DbContext.SaveChangesAsync();

        var result = await scope.Service.GetTracksByAlbumAsync(album.Id);

        result.Select(t => t.Name).Should().Equal("alpha", "Beta", "gamma", "delta");
    }

    [Fact]
    public async Task GetAllTracksAsync_Should_Return_CaseInsensitive_Order()
    {
        await using var scope = await _fixture.CreateScopeAsync();
        var now = DateTime.UtcNow;

        var artist = new Artist
        {
            Name = "Artist",
            DateAdded = now
        };

        var albums = new[]
        {
            new Album { Name = "First", Artist = artist, DateAdded = now },
            new Album { Name = "Second", Artist = artist, DateAdded = now }
        };

        var tracks = new[]
        {
            new Track { Name = "zeta", Duration = 180, Album = albums[0] },
            new Track { Name = "Alpha", Duration = 190, Album = albums[0] },
            new Track { Name = "beta", Duration = 200, Album = albums[1] },
            new Track { Name = "Gamma", Duration = 210, Album = albums[1] }
        };

        scope.DbContext.Artists.Add(artist);
        scope.DbContext.Albums.AddRange(albums);
        scope.DbContext.Tracks.AddRange(tracks);
        await scope.DbContext.SaveChangesAsync();

        var result = await scope.Service.GetAllTracksAsync();

        var expectedOrder = tracks.Select(t => t.Name)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        result.Select(t => t.Name).Should().Equal(expectedOrder);
    }
}

