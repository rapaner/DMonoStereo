using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using DMonoStereo.Models;
using DMonoStereo.Services;
using Microsoft.Maui.ApplicationModel;

namespace DMonoStereo.Views;

public partial class AlbumVersionsPage : ContentPage
{
    private readonly MusicSearchService _musicSearchService;
    private readonly MusicAlbumSearchResult _album;
    private CancellationTokenSource? _loadCts;
    private int _currentPage;
    private int _totalPages;
    private bool _isLoading;
    private bool _hasLoaded;

    public ObservableCollection<MusicAlbumVersionSummary> Versions { get; } = new();

    public string AlbumTitle => _album.Title ?? "Версии альбома";
    public bool IsPreviousEnabled => !_isLoading && _currentPage > 1;
    public bool IsNextEnabled => !_isLoading && _totalPages > 0 && _currentPage < _totalPages;
    public string PageStatusText => GetPageStatusText();
    public string EmptyStateText => GetEmptyStateText();

    public AlbumVersionsPage(MusicSearchService musicSearchService, MusicAlbumSearchResult album)
    {
        InitializeComponent();

        _musicSearchService = musicSearchService ?? throw new ArgumentNullException(nameof(musicSearchService));
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
            OnPropertyChanged(nameof(IsPreviousEnabled));
            OnPropertyChanged(nameof(IsNextEnabled));
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
}

