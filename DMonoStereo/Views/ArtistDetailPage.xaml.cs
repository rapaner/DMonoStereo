using DMonoStereo.Core.Models;
using DMonoStereo.Services;
using DMonoStereo.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;

namespace DMonoStereo.Views;

public partial class ArtistDetailPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<Task> _onChanged;
    private readonly int _artistId;

    private Artist? _artist;

    public ObservableCollection<AlbumViewModel> Albums { get; } = new();

    private int _albumCount;
    public int AlbumCount
    {
        get => _albumCount;
        private set
        {
            if (_albumCount == value)
            {
                return;
            }

            _albumCount = value;
            OnPropertyChanged(nameof(AlbumCount));
        }
    }

    private int _trackCount;
    public int TrackCount
    {
        get => _trackCount;
        private set
        {
            if (_trackCount == value)
            {
                return;
            }

            _trackCount = value;
            OnPropertyChanged(nameof(TrackCount));
        }
    }

    private double? _averageAlbumRating;
    public double? AverageAlbumRating
    {
        get => _averageAlbumRating;
        private set
        {
            if (_averageAlbumRating == value)
            {
                return;
            }

            _averageAlbumRating = value;
            OnPropertyChanged(nameof(AverageAlbumRating));
        }
    }

    private double? _averageTrackRating;
    public double? AverageTrackRating
    {
        get => _averageTrackRating;
        private set
        {
            if (_averageTrackRating == value)
            {
                return;
            }

            _averageTrackRating = value;
            OnPropertyChanged(nameof(AverageTrackRating));
        }
    }

    public ArtistDetailPage(MusicService musicService, IServiceProvider serviceProvider, int artistId, Func<Task> onChanged)
    {
        InitializeComponent();

        _musicService = musicService;
        _serviceProvider = serviceProvider;
        _artistId = artistId;
        _onChanged = onChanged;

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadArtistAsync();
    }

    private async Task LoadArtistAsync()
    {
        ResetStatistics();

        _artist = await _musicService.GetArtistByIdAsync(_artistId);
        if (_artist == null)
        {
            await DisplayAlertAsync("Ошибка", "Исполнитель не найден", "OK");
            await Navigation.PopAsync();
            return;
        }

        ArtistNameLabel.Text = _artist.Name;

        if (_artist.CoverImage != null && _artist.CoverImage.Length > 0)
        {
            ArtistCoverImage.Source = ImageSource.FromStream(() => new MemoryStream(_artist.CoverImage));
            ArtistCoverBorder.IsVisible = true;
        }
        else
        {
            ArtistCoverImage.Source = null;
            ArtistCoverBorder.IsVisible = false;
        }

        Albums.Clear();
        foreach (var album in _artist.Albums.OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase))
        {
            Albums.Add(AlbumViewModel.FromAlbum(album));
        }

        UpdateStatistics(_artist);
    }

    private async void OnAddAlbumClicked(object? sender, EventArgs e)
    {
        if (_artist == null)
        {
            return;
        }

        var page = ActivatorUtilities.CreateInstance<AddEditAlbumPage>(
            _serviceProvider,
            new Func<Task>(LoadArtistAsync),
            _artist);

        await Navigation.PushAsync(page);
    }

    private async void OnEditArtistClicked(object? sender, EventArgs e)
    {
        if (_artist == null)
        {
            return;
        }

        var page = ActivatorUtilities.CreateInstance<AddEditArtistPage>(
            _serviceProvider,
            new Func<Task>(async () =>
            {
                await LoadArtistAsync();
                await _onChanged();
            }),
            _artist);

        await Navigation.PushAsync(page);
    }

    private async void OnDeleteArtistClicked(object? sender, EventArgs e)
    {
        if (_artist == null)
        {
            return;
        }

        var confirm = await DisplayAlertAsync(
            "Удаление",
            $"Удалить исполнителя {_artist.Name}? Все альбомы и треки также будут удалены.",
            "Удалить",
            "Отмена");

        if (!confirm)
        {
            return;
        }

        await _musicService.DeleteArtistAsync(_artist.Id);
        await _onChanged();
        await Navigation.PopAsync();
    }

    private async void OnAlbumSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }

        if (e.CurrentSelection.FirstOrDefault() is not AlbumViewModel albumViewModel)
        {
            return;
        }

        var page = ActivatorUtilities.CreateInstance<AlbumDetailPage>(
            _serviceProvider,
            albumViewModel.Id,
            new Func<Task>(LoadArtistAsync));

        await Navigation.PushAsync(page);
    }

    private void ResetStatistics()
    {
        AlbumCount = 0;
        TrackCount = 0;
        AverageAlbumRating = null;
        AverageTrackRating = null;
    }

    private void UpdateStatistics(Artist artist)
    {
        AlbumCount = artist.AlbumCount;
        TrackCount = artist.TrackCount;
        AverageAlbumRating = artist.AverageAlbumRating;
        AverageTrackRating = artist.AverageTrackRating;
    }
}