using DMonoStereo.Services;
using DMonoStereo.ViewModels;
using System.Collections.ObjectModel;

namespace DMonoStereo.Views;

public partial class AllAlbumsPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly IServiceProvider _serviceProvider;

    public ObservableCollection<AlbumViewModel> Albums { get; } = new();

    private const int PageSize = 10;
    private int _currentPageIndex;
    private bool _isLoading;
    private bool _hasMore = true;
    private bool _initialLoadCompleted;

    public AllAlbumsPage(MusicService musicService, IServiceProvider serviceProvider)
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

        await LoadAlbumsAsync(reset: true);
        _initialLoadCompleted = true;
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
            var albumsPage = await _musicService.GetAlbumsPageAsync(_currentPageIndex, PageSize);

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
}