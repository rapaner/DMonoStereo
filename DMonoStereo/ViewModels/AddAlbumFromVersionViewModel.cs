using DMonoStereo.Core.Models;
using DMonoStereo.Models;
using DMonoStereo.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DMonoStereo.ViewModels;

/// <summary>
/// ViewModel для добавления альбома на основе выбранной версии (релиза).
/// </summary>
public class AddAlbumFromVersionViewModel : INotifyPropertyChanged
{
    private readonly MusicSearchService _musicSearchService;
    private readonly MusicAlbumVersionSummary _versionSummary;
    private bool _isLoading;
    private bool _hasLoaded;
    private string _artistName = string.Empty;
    private string _albumTitle = string.Empty;
    private string _year = string.Empty;
    private byte[]? _artistImageData;
    private byte[]? _coverImageData;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Коллекция треков, полученных из выбранной версии и доступных для редактирования.
    /// </summary>
    public ObservableCollection<EditableTrackViewModel> Tracks { get; } = new();

    /// <summary>
    /// Признак активной загрузки данных версии альбома.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Признак того, что данные версии были успешно загружены хотя бы один раз.
    /// </summary>
    public bool HasLoaded
    {
        get => _hasLoaded;
        private set
        {
            if (_hasLoaded != value)
            {
                _hasLoaded = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Имя артиста выбранной версии.
    /// </summary>
    public string ArtistName
    {
        get => _artistName;
        set
        {
            if (_artistName != value)
            {
                _artistName = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Название альбома/релиза выбранной версии.
    /// </summary>
    public string AlbumTitle
    {
        get => _albumTitle;
        set
        {
            if (_albumTitle != value)
            {
                _albumTitle = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Год выпуска версии в текстовом виде.
    /// </summary>
    public string Year
    {
        get => _year;
        set
        {
            if (_year != value)
            {
                _year = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Бинарные данные миниатюры артиста.
    /// </summary>
    public byte[]? ArtistImageData
    {
        get => _artistImageData;
        private set
        {
            if (_artistImageData != value)
            {
                _artistImageData = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasArtistImage));
            }
        }
    }

    /// <summary>
    /// Бинарные данные обложки альбома.
    /// </summary>
    public byte[]? CoverImageData
    {
        get => _coverImageData;
        private set
        {
            if (_coverImageData != value)
            {
                _coverImageData = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasCoverImage));
            }
        }
    }

    /// <summary>
    /// Возвращает true, если миниатюра артиста доступна.
    /// </summary>
    public bool HasArtistImage => ArtistImageData is { Length: > 0 };

    /// <summary>
    /// Возвращает true, если доступна обложка альбома.
    /// </summary>
    public bool HasCoverImage => CoverImageData is { Length: > 0 };

    /// <summary>
    /// Создаёт экземпляр модели представления.
    /// </summary>
    /// <param name="musicSearchService">Сервис поиска музыкальных данных.</param>
    /// <param name="versionSummary">Краткие данные выбранной версии альбома.</param>
    public AddAlbumFromVersionViewModel(MusicSearchService musicSearchService, MusicAlbumVersionSummary versionSummary)
    {
        _musicSearchService = musicSearchService ?? throw new ArgumentNullException(nameof(musicSearchService));
        _versionSummary = versionSummary ?? throw new ArgumentNullException(nameof(versionSummary));
    }

    /// <summary>
    /// Загружает детальную информацию о версии альбома и заполняет модель представления.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (HasLoaded || IsLoading)
        {
            return;
        }

        IsLoading = true;

        try
        {
            var detail = await _musicSearchService.GetAlbumVersionAsync(_versionSummary, cancellationToken);
            if (detail is null)
            {
                throw new InvalidOperationException("Не удалось загрузить данные о версии альбома.");
            }

            ArtistName = detail.Artist?.Name ?? string.Empty;
            AlbumTitle = detail.Title ?? string.Empty;
            Year = detail.Year?.ToString() ?? string.Empty;
            ArtistImageData = detail.Artist?.ThumbnailImageData;
            CoverImageData = detail.Image?.ImageData;

            Tracks.Clear();
            foreach (var track in detail.Tracklist)
            {
                Tracks.Add(EditableTrackViewModel.FromMusicAlbumVersionTrack(track));
            }

            HasLoaded = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Возвращает выбранные пользователем треки с повторной нумерацией.
    /// </summary>
    public IReadOnlyList<Track> BuildSelectedTracksWithRenumbering()
    {
        var selectedTracks = Tracks
            .Where(t => t.IsSelected && t.IsValid())
            .OrderBy(t => t.Position)
            .Select(t => t.ToTrack(0))
            .Where(t => t is not null)
            .Select(t => t!)
            .ToList();

        for (var i = 0; i < selectedTracks.Count; i++)
        {
            selectedTracks[i].TrackNumber = i + 1;
        }

        return selectedTracks;
    }

    /// <summary>
    /// Уведомляет представление об изменении свойства.
    /// </summary>
    /// <param name="propertyName">Имя свойства.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}