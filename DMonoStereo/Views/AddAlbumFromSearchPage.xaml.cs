using DMonoStereo.Core.Models;
using DMonoStereo.Models;
using DMonoStereo.Services;
using DMonoStereo.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DMonoStereo.Views;

public partial class AddAlbumFromSearchPage : ContentPage, INotifyPropertyChanged
{
    private readonly MusicSearchService _musicSearchService;
    private readonly MusicService _musicService;
    private readonly MusicAlbumSearchResult _searchResult;
    private MusicAlbumDetail? _albumDetail;
    private bool _isLoading;
    private string _artistName = string.Empty;
    private string _albumTitle = string.Empty;
    private string _year = string.Empty;
    private byte[]? _artistImageData;
    private byte[]? _coverImageData;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<EditableTrackViewModel> Tracks { get; } = new();

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
        set
        {
            if (_artistImageData != value)
            {
                _artistImageData = value;
                OnPropertyChanged();
            }
        }
    }

    public byte[]? CoverImageData
    {
        get => _coverImageData;
        set
        {
            if (_coverImageData != value)
            {
                _coverImageData = value;
                OnPropertyChanged();
            }
        }
    }

    public AddAlbumFromSearchPage(
        MusicSearchService musicSearchService,
        MusicService musicService,
        MusicAlbumSearchResult searchResult)
    {
        InitializeComponent();

        _musicSearchService = musicSearchService ?? throw new ArgumentNullException(nameof(musicSearchService));
        _musicService = musicService ?? throw new ArgumentNullException(nameof(musicService));
        _searchResult = searchResult ?? throw new ArgumentNullException(nameof(searchResult));

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_albumDetail == null && !_isLoading)
        {
            await LoadAlbumDetailsAsync();
        }
    }

    private async Task LoadAlbumDetailsAsync()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;

        try
        {
            _albumDetail = await _musicSearchService.GetAlbumAsync(_searchResult);

            if (_albumDetail == null)
            {
                await DisplayAlert("Ошибка", "Не удалось загрузить данные об альбоме", "OK");
                await Navigation.PopAsync();
                return;
            }

            // Заполняем данные
            ArtistName = _albumDetail.ArtistName ?? string.Empty;
            AlbumTitle = _albumDetail.Title ?? string.Empty;
            Year = _albumDetail.Year?.ToString() ?? string.Empty;
            ArtistImageData = _albumDetail.ArtistImageData;
            CoverImageData = _albumDetail.CoverImageData;

            // Заполняем треки
            Tracks.Clear();
            foreach (var track in _albumDetail.Tracks)
            {
                Tracks.Add(EditableTrackViewModel.FromMusicAlbumDetailTrack(track));
            }

            // Обновляем UI
            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnPropertyChanged(nameof(Tracks));
                UpdateImageVisibility();
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить данные об альбоме: {ex.Message}", "OK");
            await Navigation.PopAsync();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void UpdateImageVisibility()
    {
        if (ArtistImageData != null && ArtistImageData.Length > 0)
        {
            ArtistImage.IsVisible = true;
            NoArtistImageLabel.IsVisible = false;
        }
        else
        {
            ArtistImage.IsVisible = false;
            NoArtistImageLabel.IsVisible = true;
        }

        if (CoverImageData != null && CoverImageData.Length > 0)
        {
            CoverImage.IsVisible = true;
            NoCoverImageLabel.IsVisible = false;
        }
        else
        {
            CoverImage.IsVisible = false;
            NoCoverImageLabel.IsVisible = true;
        }
    }

    private async void OnAddAlbumClicked(object? sender, EventArgs e)
    {
        // Валидация полей (используем свойства, так как биндинги TwoWay)
        var artistName = ArtistName?.Trim();
        if (string.IsNullOrWhiteSpace(artistName))
        {
            await DisplayAlert("Ошибка", "Введите имя исполнителя", "OK");
            return;
        }

        var albumTitle = AlbumTitle?.Trim();
        if (string.IsNullOrWhiteSpace(albumTitle))
        {
            await DisplayAlert("Ошибка", "Введите название альбома", "OK");
            return;
        }

        int? year = null;
        if (!string.IsNullOrWhiteSpace(Year))
        {
            if (int.TryParse(Year, out var parsedYear))
            {
                year = parsedYear;
            }
            else
            {
                await DisplayAlert("Ошибка", "Введите корректный год", "OK");
                return;
            }
        }

        // Получаем выбранные треки
        var selectedTracks = Tracks
            .Where(t => t.IsSelected && t.IsValid())
            .Select(t =>
            {
                var track = t.ToTrack(0); // ID альбома будет установлен в методе AddAlbumFromSearchAsync
                return track;
            })
            .Where(t => t != null)
            .Cast<Track>()
            .ToList();

        try
        {
            // Вызываем метод добавления альбома
            await _musicService.AddAlbumFromSearchAsync(
                artistName,
                albumTitle,
                year,
                CoverImageData,
                ArtistImageData,
                selectedTracks);

            await DisplayAlert("Успех", "Альбом успешно добавлен", "OK");
            await Navigation.PopAsync();
        }
        catch (InvalidOperationException ex)
        {
            // Альбом уже существует
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось добавить альбом: {ex.Message}", "OK");
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}