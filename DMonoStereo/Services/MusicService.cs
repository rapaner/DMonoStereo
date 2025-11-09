using DMonoStereo.Core.Data;
using DMonoStereo.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DMonoStereo.Services;

/// <summary>
/// Сервис для работы с музыкальной библиотекой через Entity Framework Core
/// </summary>
public class MusicService
{
    private readonly MusicDbContext _dbContext;
    private readonly DatabaseMigrationService _migrationService;

    public MusicService(MusicDbContext dbContext, DatabaseMigrationService migrationService)
    {
        _dbContext = dbContext;
        _migrationService = migrationService;
    }

    /// <summary>
    /// Инициализация базы данных (применение миграций)
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _migrationService.MigrateAsync(cancellationToken);
    }

    #region Artists

    public async Task<List<Artist>> GetArtistsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Artists
            .Include(a => a.Albums)
            .ThenInclude(al => al.Tracks)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Artist?> GetArtistByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Artists
            .Include(a => a.Albums)
            .ThenInclude(al => al.Tracks)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Artist> AddArtistAsync(Artist artist, CancellationToken cancellationToken = default)
    {
        artist.DateAdded = DateTime.UtcNow;
        _dbContext.Artists.Add(artist);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return artist;
    }

    public async Task UpdateArtistAsync(Artist artist, CancellationToken cancellationToken = default)
    {
        _dbContext.Artists.Update(artist);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteArtistAsync(int artistId, CancellationToken cancellationToken = default)
    {
        var artist = await _dbContext.Artists.FindAsync(new object[] { artistId }, cancellationToken);
        if (artist != null)
        {
            _dbContext.Artists.Remove(artist);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    #endregion

    #region Albums

    public async Task<List<Album>> GetAlbumsByArtistAsync(int artistId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Albums
            .Where(a => a.ArtistId == artistId)
            .Include(a => a.Tracks)
            .OrderByDescending(a => a.DateAdded)
            .ToListAsync(cancellationToken);
    }

    public async Task<Album?> GetAlbumByIdAsync(int albumId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Albums
            .Include(a => a.Artist)
            .Include(a => a.Tracks)
            .FirstOrDefaultAsync(a => a.Id == albumId, cancellationToken);
    }

    public async Task<Album> AddAlbumAsync(Album album, CancellationToken cancellationToken = default)
    {
        album.DateAdded = DateTime.UtcNow;
        _dbContext.Albums.Add(album);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return album;
    }

    public async Task UpdateAlbumAsync(Album album, CancellationToken cancellationToken = default)
    {
        _dbContext.Albums.Update(album);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAlbumAsync(int albumId, CancellationToken cancellationToken = default)
    {
        var album = await _dbContext.Albums.FindAsync(new object[] { albumId }, cancellationToken);
        if (album != null)
        {
            _dbContext.Albums.Remove(album);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    #endregion

    #region Tracks

    public async Task<List<Track>> GetTracksByAlbumAsync(int albumId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tracks
            .Where(t => t.AlbumId == albumId)
            .OrderBy(t => t.TrackNumber ?? int.MaxValue)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Track?> GetTrackByIdAsync(int trackId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tracks.FirstOrDefaultAsync(t => t.Id == trackId, cancellationToken);
    }

    public async Task<Track> AddTrackAsync(Track track, CancellationToken cancellationToken = default)
    {
        _dbContext.Tracks.Add(track);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return track;
    }

    public async Task UpdateTrackAsync(Track track, CancellationToken cancellationToken = default)
    {
        _dbContext.Tracks.Update(track);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTrackAsync(int trackId, CancellationToken cancellationToken = default)
    {
        var track = await _dbContext.Tracks.FindAsync(new object[] { trackId }, cancellationToken);
        if (track != null)
        {
            _dbContext.Tracks.Remove(track);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    #endregion
}
