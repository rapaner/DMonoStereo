using DMonoStereo.Core.Models;
using DMonoStereo.Services;
using DMonoStereo.ViewModels;
using DMonoStereo.Views;
using System.Collections.ObjectModel;

namespace DMonoStereo;

public partial class MainPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly IServiceProvider _serviceProvider;
    private bool _isInitialized;

    public ObservableCollection<ArtistViewModel> Artists { get; } = new();
    private readonly List<Artist> _artistModels = new();

    public MainPage(MusicService musicService, IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _musicService = musicService;
        _serviceProvider = serviceProvider;

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isInitialized)
        {
            await _musicService.InitializeAsync();
            _isInitialized = true;
        }

        await LoadArtistsAsync();
    }

    private async Task LoadArtistsAsync()
    {
        var artists = await _musicService.GetArtistsAsync();

        Artists.Clear();
        _artistModels.Clear();

        foreach (var artist in artists)
        {
            Artists.Add(ArtistViewModel.FromArtist(artist));
            _artistModels.Add(artist);
        }

        UpdateCounters();
    }

    private void UpdateCounters()
    {
        var artistCount = _artistModels.Count;
        var albumCount = _artistModels.Sum(a => a.Albums.Count);
        var trackCount = _artistModels.Sum(a => a.Albums.Sum(al => al.Tracks.Count));

        ArtistsCountLabel.Text = artistCount > 0 ? $"Всего: {artistCount}" : "Нет данных";
        AlbumsCountLabel.Text = albumCount > 0 ? $"Всего: {albumCount}" : "Нет данных";
        TracksCountLabel.Text = trackCount > 0 ? $"Всего: {trackCount}" : "Нет данных";
    }

    private async void OnAddAlbumClicked(object? sender, EventArgs e)
    {
        var page = ActivatorUtilities.CreateInstance<AddEditAlbumPage>(
            _serviceProvider,
            new Func<Task>(LoadArtistsAsync));

        await Navigation.PushAsync(page);
    }

    private async void OnSearchAlbumClicked(object? sender, EventArgs e)
    {
        var page = ActivatorUtilities.CreateInstance<AlbumSearchPage>(_serviceProvider);
        await Navigation.PushAsync(page);
    }

    private async void OnAllArtistsTapped(object? sender, TappedEventArgs e)
    {
        var page = ActivatorUtilities.CreateInstance<AllArtistsPage>(_serviceProvider);
        await Navigation.PushAsync(page);
    }

    private async void OnAllAlbumsTapped(object? sender, TappedEventArgs e)
    {
        var page = ActivatorUtilities.CreateInstance<AllAlbumsPage>(_serviceProvider);
        await Navigation.PushAsync(page);
    }

    private async void OnAllTracksTapped(object? sender, TappedEventArgs e)
    {
        var page = ActivatorUtilities.CreateInstance<AllTracksPage>(_serviceProvider);
        await Navigation.PushAsync(page);
    }

    private async void OnSettingsTapped(object? sender, TappedEventArgs e)
    {
        var page = ActivatorUtilities.CreateInstance<SettingsPage>(_serviceProvider);
        await Navigation.PushAsync(page);
    }
}