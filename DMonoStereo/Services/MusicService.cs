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
            .OrderBy(a => a.Name.ToLower())
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Artist>> GetArtistsPageAsync(int pageIndex, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        var query = _dbContext.Artists
            .Include(a => a.Albums)
            .ThenInclude(al => al.Tracks)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var filter = $"%{searchTerm.Trim()}%";
            query = query.Where(a => EF.Functions.Like(a.Name, filter));
        }

        return await query
            .OrderBy(a => a.Name.ToLower())
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
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

    public async Task<bool> ArtistExistsByNameAsync(string name, int? excludeArtistId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var normalizedName = name.Trim().ToLower();

        var query = _dbContext.Artists.AsQueryable();

        if (excludeArtistId.HasValue)
        {
            query = query.Where(a => a.Id != excludeArtistId.Value);
        }

        return await query.AnyAsync(a => a.Name.ToLower() == normalizedName, cancellationToken);
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

    #endregion Artists

    #region Albums

    public async Task<List<Album>> GetAlbumsByArtistAsync(int artistId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Albums
            .Where(a => a.ArtistId == artistId)
            .Include(a => a.Tracks)
            .OrderByDescending(a => a.DateAdded)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Album>> GetAllAlbumsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Albums
            .Include(a => a.Artist)
            .Include(a => a.Tracks)
            .OrderBy(a => a.Name.ToLower())
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AlbumExistsForArtistAsync(
        int artistId,
        string name,
        int? excludeAlbumId = null,
        CancellationToken cancellationToken = default)
    {
        if (artistId <= 0 || string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var normalizedName = name.Trim().ToLower();

        var query = _dbContext.Albums
            .Where(a => a.ArtistId == artistId);

        if (excludeAlbumId.HasValue)
        {
            query = query.Where(a => a.Id != excludeAlbumId.Value);
        }

        return await query.AnyAsync(
            a => a.Name.ToLower() == normalizedName,
            cancellationToken);
    }

    public async Task<List<Album>> GetAlbumsPageAsync(int pageIndex, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        var query = _dbContext.Albums
            .Include(a => a.Artist)
            .Include(a => a.Tracks)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var filter = $"%{searchTerm.Trim()}%";
            query = query.Where(a =>
                EF.Functions.Like(a.Name, filter) ||
                (a.Artist != null && EF.Functions.Like(a.Artist.Name, filter)));
        }

        return await query
            .OrderBy(a => a.Name.ToLower())
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
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

    #endregion Albums

    #region Tracks

    public async Task<List<Track>> GetTracksByAlbumAsync(int albumId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tracks
            .Where(t => t.AlbumId == albumId)
            .OrderBy(t => t.TrackNumber ?? int.MaxValue)
            .ThenBy(t => t.Name.ToLower())
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Track>> GetAllTracksAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Tracks
            .Include(t => t.Album)
            .ThenInclude(a => a.Artist)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var filter = $"%{searchTerm.Trim()}%";
            query = query.Where(t =>
                EF.Functions.Like(t.Name, filter) ||
                (t.Album != null && EF.Functions.Like(t.Album.Name, filter)) ||
                (t.Album != null && t.Album.Artist != null && EF.Functions.Like(t.Album.Artist.Name, filter)));
        }

        return await query
            .OrderBy(t => t.Name.ToLower())
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

    #endregion Tracks

    #region Maintenance

    public async Task ClearLibraryAsync(CancellationToken cancellationToken = default)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        _dbContext.Tracks.RemoveRange(_dbContext.Tracks);
        _dbContext.Albums.RemoveRange(_dbContext.Albums);
        _dbContext.Artists.RemoveRange(_dbContext.Artists);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    #endregion Maintenance

    #region Album From Search

    /// <summary>
    /// Добавляет альбом из результатов поиска с проверкой существования исполнителя и альбома.
    /// </summary>
    /// <param name="artistName">Имя исполнителя.</param>
    /// <param name="albumName">Название альбома.</param>
    /// <param name="year">Год выпуска альбома.</param>
    /// <param name="coverImage">Обложка альбома.</param>
    /// <param name="artistImage">Изображение исполнителя.</param>
    /// <param name="tracks">Список выбранных треков для добавления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Добавленный альбом.</returns>
    /// <exception cref="ArgumentException">Если альбом уже существует у исполнителя.</exception>
    public async Task<Album> AddAlbumFromSearchAsync(
        string artistName,
        string albumName,
        int? year,
        byte[]? coverImage,
        byte[]? artistImage,
        IReadOnlyList<Core.Models.Track> tracks,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(artistName))
        {
            throw new ArgumentException("Имя исполнителя не может быть пустым", nameof(artistName));
        }

        if (string.IsNullOrWhiteSpace(albumName))
        {
            throw new ArgumentException("Название альбома не может быть пустым", nameof(albumName));
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Проверяем существование исполнителя
            Artist artist;
            var normalizedArtistName = artistName.Trim();
            var exists = await ArtistExistsByNameAsync(normalizedArtistName, cancellationToken: cancellationToken);

            if (exists)
            {
                // Получаем существующего исполнителя напрямую из контекста
                artist = await _dbContext.Artists
                    .FirstOrDefaultAsync(a => a.Name.ToLower() == normalizedArtistName.ToLower(), cancellationToken)
                    ?? throw new InvalidOperationException($"Исполнитель '{normalizedArtistName}' не найден, хотя должен существовать");
                
                // Если у исполнителя нет изображения, но оно было предоставлено, обновляем его
                if (artist.CoverImage == null && artistImage != null)
                {
                    artist.CoverImage = artistImage;
                    _dbContext.Artists.Update(artist);
                }
            }
            else
            {
                // Создаем нового исполнителя
                artist = new Artist
                {
                    Name = normalizedArtistName,
                    CoverImage = artistImage,
                    DateAdded = DateTime.UtcNow
                };
                _dbContext.Artists.Add(artist);
            }

            // Сохраняем изменения, чтобы получить ID исполнителя
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Проверяем существование альбома у исполнителя
            if (await AlbumExistsForArtistAsync(artist.Id, albumName, cancellationToken: cancellationToken))
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new InvalidOperationException($"У исполнителя '{artistName}' уже есть альбом с названием '{albumName}'");
            }

            // Создаем альбом
            var album = new Album
            {
                Name = albumName.Trim(),
                Year = year,
                CoverImage = coverImage,
                ArtistId = artist.Id,
                DateAdded = DateTime.UtcNow
            };

            _dbContext.Albums.Add(album);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Добавляем треки
            if (tracks != null && tracks.Count > 0)
            {
                foreach (var track in tracks)
                {
                    track.AlbumId = album.Id;
                    _dbContext.Tracks.Add(track);
                }
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return album;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    #endregion Album From Search
}