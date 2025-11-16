using DMonoStereo.Services;
using DMonoStereo.ViewModels;
using System.Collections.ObjectModel;
using DMonoStereo.Models;
using System.Linq;

namespace DMonoStereo.Views;

public partial class AllTracksPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly IServiceProvider _serviceProvider;

    public ObservableCollection<TrackListItemViewModel> Tracks { get; } = new();
    public ObservableCollection<AllTrackSortOption> SortOptions { get; } = new()
    {
        new(AllTracksSortOption.Name, "По названию"),
        new(AllTracksSortOption.RatingDescending, "По рейтингу (↓)")
    };

    private bool _isLoading;
    private string? _currentFilter;
    private AllTracksSortOption _currentSortOption = AllTracksSortOption.Name;

    private AllTrackSortOption? _selectedSortOption;
    public AllTrackSortOption? SelectedSortOption
    {
        get => _selectedSortOption;
        set
        {
            if (value is null || _selectedSortOption == value)
            {
                return;
            }

            _selectedSortOption = value;
            OnPropertyChanged();
        }
    }

    public AllTracksPage(MusicService musicService, IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _musicService = musicService;
        _serviceProvider = serviceProvider;

        BindingContext = this;

        SelectedSortOption = SortOptions.FirstOrDefault();
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
            var tracks = await _musicService.GetAllTracksAsync(_currentFilter, _currentSortOption);

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

    private async void OnSortOptionChanged(object? sender, EventArgs e)
    {
        if (sender is not Picker picker || picker.SelectedItem is not AllTrackSortOption selectedOption)
        {
            return;
        }

        if (_currentSortOption == selectedOption.Option)
        {
            return;
        }

        _currentSortOption = selectedOption.Option;
        SelectedSortOption = selectedOption;
        await LoadTracksAsync();
    }
}

public record AllTrackSortOption(AllTracksSortOption Option, string DisplayName);