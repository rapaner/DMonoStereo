using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DMonoStereo.Core.Models;
using DMonoStereo.Models;
using DMonoStereo.Services;

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

    public ObservableCollection<EditableTrackViewModel> Tracks { get; } = new();

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

    public bool HasArtistImage => ArtistImageData is { Length: > 0 };
    public bool HasCoverImage => CoverImageData is { Length: > 0 };

    public AddAlbumFromVersionViewModel(MusicSearchService musicSearchService, MusicAlbumVersionSummary versionSummary)
    {
        _musicSearchService = musicSearchService ?? throw new ArgumentNullException(nameof(musicSearchService));
        _versionSummary = versionSummary ?? throw new ArgumentNullException(nameof(versionSummary));
    }

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

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


