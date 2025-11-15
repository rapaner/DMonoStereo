using DMonoStereo.Models;
using DMonoStereo.Services;
using DMonoStereo.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;

namespace DMonoStereo.Views;

public partial class AllArtistsPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly IServiceProvider _serviceProvider;

    public ObservableCollection<ArtistViewModel> Artists { get; } = new();
    public ObservableCollection<ArtistSortOptionItem> SortOptions { get; } = new()
    {
        new(ArtistSortOption.Name, "По имени"),
        new(ArtistSortOption.TrackRatingDescending, "По рейтингу треков (↓)")
    };

    private const int PageSize = 10;
    private int _currentPageIndex;
    private bool _isLoading;
    private bool _hasMore = true;
    private bool _initialLoadCompleted;
    private string? _currentFilter;
    private ArtistSortOption _currentSortOption = ArtistSortOption.Name;

    private ArtistSortOptionItem? _selectedSortOption;
    public ArtistSortOptionItem? SelectedSortOption
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

    public AllArtistsPage(MusicService musicService, IServiceProvider serviceProvider)
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

        if (_initialLoadCompleted)
        {
            return;
        }

        await LoadArtistsAsync(reset: true);
        _initialLoadCompleted = true;
    }

    private async Task LoadArtistsAsync(bool reset = false)
    {
        if (_isLoading)
        {
            return;
        }

        if (reset)
        {
            _currentPageIndex = 0;
            _hasMore = true;
            Artists.Clear();
        }

        if (!_hasMore)
        {
            return;
        }

        try
        {
            _isLoading = true;
            var artistsPage = await _musicService.GetArtistsPageAsync(
                _currentPageIndex,
                PageSize,
                _currentFilter,
                _currentSortOption);

            foreach (var artist in artistsPage)
            {
                Artists.Add(ArtistViewModel.FromArtist(artist));
            }

            if (artistsPage.Count < PageSize)
            {
                _hasMore = false;
            }
            else
            {
                _currentPageIndex++;
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async void OnAddArtistClicked(object? sender, EventArgs e)
    {
        var page = ActivatorUtilities.CreateInstance<AddEditArtistPage>(
            _serviceProvider,
            new Func<Task>(() => LoadArtistsAsync(reset: true)));

        await Navigation.PushAsync(page);
    }

    private async void OnArtistSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }

        if (e.CurrentSelection.FirstOrDefault() is not ArtistViewModel selectedArtist)
        {
            return;
        }

        var page = ActivatorUtilities.CreateInstance<ArtistDetailPage>(
            _serviceProvider,
            selectedArtist.Id,
            new Func<Task>(() => LoadArtistsAsync(reset: true)));

        await Navigation.PushAsync(page);
    }

    private async void OnRemainingItemsThresholdReached(object? sender, EventArgs e)
    {
        await LoadArtistsAsync();
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
        await LoadArtistsAsync(reset: true);
    }

    private async void OnSortOptionChanged(object? sender, EventArgs e)
    {
        if (sender is not Picker picker || picker.SelectedItem is not ArtistSortOptionItem selectedOption)
        {
            return;
        }

        if (_currentSortOption == selectedOption.Option)
        {
            return;
        }

        _currentSortOption = selectedOption.Option;
        SelectedSortOption = selectedOption;
        await LoadArtistsAsync(reset: true);
    }
}

public record ArtistSortOptionItem(ArtistSortOption Option, string DisplayName);