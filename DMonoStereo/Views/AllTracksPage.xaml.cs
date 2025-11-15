using DMonoStereo.Services;
using DMonoStereo.ViewModels;
using System.Collections.ObjectModel;

namespace DMonoStereo.Views;

public partial class AllTracksPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly IServiceProvider _serviceProvider;

    public ObservableCollection<TrackListItemViewModel> Tracks { get; } = new();

    private bool _isLoading;
    private string? _currentFilter;

    public AllTracksPage(MusicService musicService, IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _musicService = musicService;
        _serviceProvider = serviceProvider;

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTracksAsync();
    }

    private async Task LoadTracksAsync()
    {
        if (_isLoading)
        {
            return;
        }

        try
        {
            _isLoading = true;
            var tracks = await _musicService.GetAllTracksAsync(_currentFilter);

            Tracks.Clear();
            foreach (var track in tracks)
            {
                Tracks.Add(TrackListItemViewModel.FromTrack(track));
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async void OnTrackSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }

        if (e.CurrentSelection.FirstOrDefault() is not TrackListItemViewModel trackViewModel)
        {
            return;
        }

        var track = await _musicService.GetTrackByIdAsync(trackViewModel.Id);
        if (track is null)
        {
            await DisplayAlertAsync("Ошибка", "Не удалось загрузить данные трека.", "OK");
            return;
        }

        var album = await _musicService.GetAlbumByIdAsync(track.AlbumId);
        if (album is null)
        {
            await DisplayAlertAsync("Ошибка", "Не удалось найти альбом выбранного трека.", "OK");
            return;
        }

        var page = ActivatorUtilities.CreateInstance<AddEditTrackPage>(
            _serviceProvider,
            album,
            new Func<Task>(LoadTracksAsync),
            track);

        await Navigation.PushAsync(page);
    }

    private async void OnFilterTextChanged(object? sender, TextChangedEventArgs e)
    {
        var newFilter = string.IsNullOrWhiteSpace(e.NewTextValue)
            ? null
            : e.NewTextValue!.Trim();

        if (string.Equals(_currentFilter, newFilter, StringComparison.Ordinal))
        {
            return;
        }

        _currentFilter = newFilter;
        await LoadTracksAsync();
    }
}