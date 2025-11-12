using DMonoStereo.Core.Models;
using DMonoStereo.Tests.Infrastructure;
using FluentAssertions;
using System.Linq;

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
    public async Task GetArtistsPageAsync_Should_Filter_By_Name()
    {
        await using var scope = await _fixture.CreateScopeAsync();
        var now = DateTime.UtcNow;

        var artists = new[]
        {
            new Artist { Name = "Alpha", DateAdded = now },
            new Artist { Name = "Bravo", DateAdded = now },
            new Artist { Name = "Gamma", DateAdded = now },
            new Artist { Name = "Alchemist", DateAdded = now }
        };

        scope.DbContext.Artists.AddRange(artists);
        await scope.DbContext.SaveChangesAsync();

        var result = await scope.Service.GetArtistsPageAsync(pageIndex: 0, pageSize: 10, searchTerm: "al");

        result.Should().HaveCount(2);
        result.Select(a => a.Name).Should().BeEquivalentTo("Alpha", "Alchemist");
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
    public async Task GetAlbumsPageAsync_Should_Filter_By_Name_Or_Artist()
    {
        await using var scope = await _fixture.CreateScopeAsync();
        var now = DateTime.UtcNow;

        var artists = new[]
        {
            new Artist { Name = "First Artist", DateAdded = now },
            new Artist { Name = "Second Artist", DateAdded = now }
        };

        var albums = new[]
        {
            new Album { Name = "Alpha Album", Artist = artists[0], DateAdded = now },
            new Album { Name = "Beta Album", Artist = artists[0], DateAdded = now },
            new Album { Name = "Hidden Gem", Artist = artists[1], DateAdded = now }
        };

        scope.DbContext.Artists.AddRange(artists);
        scope.DbContext.Albums.AddRange(albums);
        await scope.DbContext.SaveChangesAsync();

        var resultByAlbum = await scope.Service.GetAlbumsPageAsync(pageIndex: 0, pageSize: 10, searchTerm: "Alpha");
        resultByAlbum.Should().ContainSingle();
        resultByAlbum.First().Name.Should().Be("Alpha Album");

        var resultByArtist = await scope.Service.GetAlbumsPageAsync(pageIndex: 0, pageSize: 10, searchTerm: "Second");
        resultByArtist.Should().ContainSingle();
        resultByArtist.First().Name.Should().Be("Hidden Gem");
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

    [Fact]
    public async Task GetAllTracksAsync_Should_Filter_By_Track_Album_Or_Artist()
    {
        await using var scope = await _fixture.CreateScopeAsync();
        var now = DateTime.UtcNow;

        var artists = new[]
        {
            new Artist { Name = "Alpha Artist", DateAdded = now },
            new Artist { Name = "Beta Band", DateAdded = now }
        };

        var albums = new[]
        {
            new Album { Name = "Greatest Hits", Artist = artists[0], DateAdded = now },
            new Album { Name = "Live Session", Artist = artists[1], DateAdded = now }
        };

        var tracks = new[]
        {
            new Track { Name = "Sunrise", Duration = 200, Album = albums[0] },
            new Track { Name = "Midnight Jam", Duration = 210, Album = albums[1] }
        };

        scope.DbContext.Artists.AddRange(artists);
        scope.DbContext.Albums.AddRange(albums);
        scope.DbContext.Tracks.AddRange(tracks);
        await scope.DbContext.SaveChangesAsync();

        var byTrack = await scope.Service.GetAllTracksAsync(searchTerm: "sun");
        byTrack.Should().ContainSingle();
        byTrack.First().Name.Should().Be("Sunrise");

        var byAlbum = await scope.Service.GetAllTracksAsync(searchTerm: "Live");
        byAlbum.Should().ContainSingle();
        byAlbum.First().Name.Should().Be("Midnight Jam");

        var byArtist = await scope.Service.GetAllTracksAsync(searchTerm: "Alpha");
        byArtist.Should().ContainSingle();
        byArtist.First().Name.Should().Be("Sunrise");
    }
}

