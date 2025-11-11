using DMonoStereo.Services;
using DMonoStereo.ViewModels;
using System.Collections.ObjectModel;

namespace DMonoStereo.Views;

public partial class AllArtistsPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly IServiceProvider _serviceProvider;

    public ObservableCollection<ArtistViewModel> Artists { get; } = new();

    private const int PageSize = 10;
    private int _currentPageIndex;
    private bool _isLoading;
    private bool _hasMore = true;
    private bool _initialLoadCompleted;

    public AllArtistsPage(MusicService musicService, IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _musicService = musicService;
        _serviceProvider = serviceProvider;

        BindingContext = this;
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
            var artistsPage = await _musicService.GetArtistsPageAsync(_currentPageIndex, PageSize);

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
}