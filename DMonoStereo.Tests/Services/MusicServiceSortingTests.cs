using DMonoStereo.Core.Models;
using DMonoStereo.Models;
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
    public async Task GetArtistsPageAsync_Should_Sort_By_Name_When_Explicit_Option_Selected()
    {
        await using var scope = await _fixture.CreateScopeAsync();
        var now = DateTime.UtcNow;

        var artistNames = new[] { "delta", "Charlie", "bravo", "Alpha" };
        var artists = artistNames.Select((name, index) => new Artist
        {
            Name = name,
            DateAdded = now.AddMinutes(index * -1)
        }).ToArray();

        scope.DbContext.Artists.AddRange(artists);
        await scope.DbContext.SaveChangesAsync();

        var result = await scope.Service.GetArtistsPageAsync(
            pageIndex: 0,
            pageSize: artists.Length,
            sortOption: AllArtistsSortOption.Name);

        var expectedOrder = artistNames.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToArray();
        result.Select(a => a.Name).Should().Equal(expectedOrder);
    }

    [Fact]
    public async Task GetArtistsPageAsync_Should_Sort_By_TrackRatingDescending()
    {
        await using var scope = await _fixture.CreateScopeAsync();
        var now = DateTime.UtcNow;

        var unrated = BuildArtistWithRatings("No Rating", now, Array.Empty<int?>());
        var medium = BuildArtistWithRatings("Medium", now, new int?[] { 6, 7 });
        var high = BuildArtistWithRatings("High", now, new int?[] { 9, 8, 10 });
        var low = BuildArtistWithRatings("Low", now, new int?[] { 4 });

        scope.DbContext.Artists.AddRange(unrated, medium, high, low);
        await scope.DbContext.SaveChangesAsync();

        var result = await scope.Service.GetArtistsPageAsync(
            pageIndex: 0,
            pageSize: 10,
            sortOption: AllArtistsSortOption.TrackRatingDescending);

        result.Select(a => a.Name).Should().Equal("High", "Medium", "Low", "No Rating");
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

    private static Artist BuildArtistWithRatings(string name, DateTime timestamp, IReadOnlyList<int?> ratings)
    {
        var artist = new Artist
        {
            Name = name,
            DateAdded = timestamp
        };

        var album = new Album
        {
            Name = $"{name} Album",
            Artist = artist,
            DateAdded = timestamp
        };

        if (ratings.Count == 0)
        {
            album.Tracks.Add(new Track
            {
                Name = $"{name} Track 0",
                Duration = 180,
                Album = album
            });
        }
        else
        {
            foreach (var (rating, index) in ratings.Select((value, idx) => (value, idx)))
            {
                album.Tracks.Add(new Track
                {
                    Name = $"{name} Track {index}",
                    Duration = 180,
                    Rating = rating,
                    Album = album
                });
            }
        }

        artist.Albums.Add(album);

        return artist;
    }
}