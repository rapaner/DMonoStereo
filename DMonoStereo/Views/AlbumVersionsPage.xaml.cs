using DMonoStereo.Models;
using DMonoStereo.Services;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace DMonoStereo.Views;

public partial class AlbumVersionsPage : ContentPage
{
    private readonly MusicSearchService _musicSearchService;
    private readonly IServiceProvider _serviceProvider;
    private readonly MusicAlbumSearchResult _album;
    private CancellationTokenSource? _loadCts;
    private int _currentPage;
    private int _totalPages;
    private bool _isLoading;
    private bool _hasLoaded;

    public ObservableCollection<MusicAlbumVersionSummary> Versions { get; } = new();

    public string AlbumTitle => _album.Title ?? "Версии альбома";
    public bool IsFirstEnabled => !_isLoading && _currentPage > 1;
    public bool IsPreviousEnabled => !_isLoading && _currentPage > 1;
    public bool IsNextEnabled => !_isLoading && _totalPages > 0 && _currentPage < _totalPages;
    public bool IsLastEnabled => !_isLoading && _totalPages > 0 && _currentPage < _totalPages;
    public string PageStatusText => GetPageStatusText();
    public string EmptyStateText => GetEmptyStateText();

    public AlbumVersionsPage(
        MusicSearchService musicSearchService,
        IServiceProvider serviceProvider,
        MusicAlbumSearchResult album)
    {
        InitializeComponent();

        _musicSearchService = musicSearchService ?? throw new ArgumentNullException(nameof(musicSearchService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _album = album ?? throw new ArgumentNullException(nameof(album));

        BindingContext = this;
        Title = AlbumTitle;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_hasLoaded)
        {
            return;
        }

        if (_album.MasterId is null or <= 0)
        {
            UpdateUiState();
            _hasLoaded = true;
            return;
        }

        _ = NavigateToPageAsync(1);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        CancelLoading();
    }

    private async void OnPreviousClicked(object? sender, EventArgs e)
    {
        if (!IsPreviousEnabled)
        {
            return;
        }

        await NavigateToPageAsync(_currentPage - 1);
    }

    private async void OnNextClicked(object? sender, EventArgs e)
    {
        if (!IsNextEnabled)
        {
            return;
        }

        await NavigateToPageAsync(_currentPage + 1);
    }

    private async void OnFirstClicked(object? sender, EventArgs e)
    {
        if (!IsFirstEnabled)
        {
            return;
        }

        await NavigateToPageAsync(1);
    }

    private async void OnLastClicked(object? sender, EventArgs e)
    {
        if (!IsLastEnabled)
        {
            return;
        }

        await NavigateToPageAsync(_totalPages);
    }

    private async Task NavigateToPageAsync(int page)
    {
        CancelLoading();

        _loadCts = new CancellationTokenSource();

        try
        {
            await LoadVersionsAsync(page, _loadCts.Token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task LoadVersionsAsync(int page, CancellationToken cancellationToken)
    {
        if (page < 1)
        {
            page = 1;
        }

        _isLoading = true;
        UpdateUiState();

        try
        {
            var response = await _musicSearchService.GetAlbumVersionsAsync(_album, page, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            Versions.Clear();
            foreach (var version in response.Versions)
            {
                Versions.Add(version);
            }

            _totalPages = response.Pagination?.Pages ?? 0;
            if (_totalPages == 0 && Versions.Count > 0)
            {
                _totalPages = 1;
            }

            _currentPage = Versions.Count > 0 ? page : (_totalPages > 0 ? Math.Min(page, _totalPages) : 0);
            _hasLoaded = true;
        }
        finally
        {
            _isLoading = false;
            UpdateUiState();
        }
    }

    private void CancelLoading()
    {
        if (_loadCts is null)
        {
            return;
        }

        if (!_loadCts.IsCancellationRequested)
        {
            _loadCts.Cancel();
        }

        _loadCts.Dispose();
        _loadCts = null;
    }

    private void UpdateUiState()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            OnPropertyChanged(nameof(IsFirstEnabled));
            OnPropertyChanged(nameof(IsPreviousEnabled));
            OnPropertyChanged(nameof(IsNextEnabled));
            OnPropertyChanged(nameof(IsLastEnabled));
            OnPropertyChanged(nameof(PageStatusText));
            OnPropertyChanged(nameof(EmptyStateText));
        });
    }

    private string GetPageStatusText()
    {
        if (_album.MasterId is null or <= 0)
        {
            return "Для этого альбома нет версий.";
        }

        if (_isLoading)
        {
            return "Загрузка версий...";
        }

        if (_totalPages == 0)
        {
            return "Версии не найдены";
        }

        return $"Страница {_currentPage} из {_totalPages}";
    }

    private string GetEmptyStateText()
    {
        if (_album.MasterId is null or <= 0)
        {
            return "Версии недоступны.";
        }

        if (_isLoading)
        {
            return "Загрузка версий...";
        }

        if (Versions.Count == 0)
        {
            return "Версии не найдены";
        }

        return string.Empty;
    }

    private async void OnVersionSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }

        if (e.CurrentSelection.FirstOrDefault() is not MusicAlbumVersionSummary version)
        {
            return;
        }

        var page = ActivatorUtilities.CreateInstance<AddAlbumFromVersionPage>(_serviceProvider, version);
        await Navigation.PushAsync(page);
    }

    private async void OnPageStatusTapped(object? sender, TappedEventArgs e)
    {
        if (_isLoading || _totalPages <= 0)
        {
            return;
        }

        while (true)
        {
            var input = await DisplayPromptAsync(
                "Перейти к странице",
                $"1..{_totalPages}",
                "OK",
                "Отмена",
                keyboard: Keyboard.Numeric);

            if (input is null || string.IsNullOrWhiteSpace(input))
            {
                return; // отмена или пусто
            }

            if (int.TryParse(input, out var pageNumber) && pageNumber >= 1 && pageNumber <= _totalPages)
            {
                await NavigateToPageAsync(pageNumber);
                return;
            }

            await DisplayAlert("Ошибка", $"Введите число от 1 до {_totalPages}.", "OK");
        }
    }
}