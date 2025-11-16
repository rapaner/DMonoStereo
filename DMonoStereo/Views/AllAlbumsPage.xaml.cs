using DMonoStereo.Services;
using DMonoStereo.Models;
using DMonoStereo.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;

namespace DMonoStereo.Views;

public partial class AllAlbumsPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly IServiceProvider _serviceProvider;
    private CancellationTokenSource? _debounceCts;
    private const int SearchDelayMs = 1000;

    public ObservableCollection<AlbumViewModel> Albums { get; } = new();
    public ObservableCollection<AllAlbumSortOption> SortOptions { get; } = new()
    {
        new(AllAlbumsSortOption.Name, "По названию"),
        new(AllAlbumsSortOption.TrackRatingDescending, "По рейтингу треков (↓)")
    };

    private const int PageSize = 10;
    private int _currentPageIndex;
    private bool _isLoading;
    private bool _hasMore = true;
    private string? _currentFilter;
    private AllAlbumsSortOption _currentSortOption = AllAlbumsSortOption.Name;

    private AllAlbumSortOption? _selectedSortOption;
    public AllAlbumSortOption? SelectedSortOption
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

    public AllAlbumsPage(MusicService musicService, IServiceProvider serviceProvider)
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

        await LoadAlbumsAsync(reset: true);
    }

    private async Task LoadAlbumsAsync(bool reset = false)
    {
        if (_isLoading)
        {
            return;
        }

        if (reset)
        {
            _currentPageIndex = 0;
            _hasMore = true;
            Albums.Clear();
        }

        if (!_hasMore)
        {
            return;
        }

        try
        {
            _isLoading = true;
            var albumsPage = await _musicService.GetAlbumsPageAsync(
                _currentPageIndex,
                PageSize,
                _currentFilter,
                _currentSortOption);

            foreach (var album in albumsPage)
            {
                Albums.Add(AlbumViewModel.FromAlbum(album));
            }

            if (albumsPage.Count < PageSize)
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
            new Func<Task>(() => LoadAlbumsAsync(reset: true)));

        await Navigation.PushAsync(page);
    }

    private async void OnRemainingItemsThresholdReached(object? sender, EventArgs e)
    {
        await LoadAlbumsAsync();
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
                await LoadAlbumsAsync(reset: true);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async void OnSortOptionChanged(object? sender, EventArgs e)
    {
        if (sender is not Picker picker || picker.SelectedItem is not AllAlbumSortOption selectedOption)
        {
            return;
        }

        if (_currentSortOption == selectedOption.Option)
        {
            return;
        }

        _currentSortOption = selectedOption.Option;
        SelectedSortOption = selectedOption;
        await LoadAlbumsAsync(reset: true);
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

public record AllAlbumSortOption(AllAlbumsSortOption Option, string DisplayName);