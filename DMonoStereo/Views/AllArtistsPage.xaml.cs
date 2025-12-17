using DMonoStereo.Models;
using DMonoStereo.Services;
using DMonoStereo.ViewModels;
using System.Collections.ObjectModel;

namespace DMonoStereo.Views;

public partial class AllArtistsPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly IServiceProvider _serviceProvider;
    private CancellationTokenSource? _debounceCts;
    private const int SearchDelayMs = 1000;

    public ObservableCollection<ArtistViewModel> Artists { get; } = [];

    public ObservableCollection<ArtistSortOptionItem> SortOptions { get; } =
    [
        new(AllArtistsSortOption.Name, "По имени"),
        new(AllArtistsSortOption.TrackRatingDescending, "По рейтингу треков (↓)")
    ];

    private const int PageSize = 10;
    private int _currentPageIndex;
    private bool _isLoading;
    private bool _hasMore = true;
    private string? _currentFilter;
    private AllArtistsSortOption _currentSortOption = AllArtistsSortOption.Name;

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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        CancelDebounce();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadArtistsAsync(reset: true);
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
        catch(Exception ex)
        {
            await DisplayAlertAsync("Ошибка", ex.Message, "Отмена");
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

        CancelDebounce();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        try
        {
            await Task.Delay(SearchDelayMs, token);
            if (!token.IsCancellationRequested)
            {
                await LoadArtistsAsync(reset: true);
            }
        }
        catch (OperationCanceledException)
        {
        }
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

    private void CancelDebounce()
    {
        if (_debounceCts is null)
        {
            return;
        }

        if (!_debounceCts.IsCancellationRequested)
        {
            _debounceCts.Cancel();
        }

        _debounceCts.Dispose();
        _debounceCts = null;
    }
}

public record ArtistSortOptionItem(AllArtistsSortOption Option, string DisplayName);