using System;
using DMonoStereo.Models;
using DMonoStereo.Services;
using Microsoft.Maui.ApplicationModel;
using System.Collections.ObjectModel;

namespace DMonoStereo.Views;

public partial class AlbumSearchPage : ContentPage
{
    private readonly MusicSearchService _musicSearchService;
    private CancellationTokenSource? _searchCts;
    private CancellationTokenSource? _debounceCts;
    private string _currentQuery = string.Empty;
    private int _currentPage;
    private int _totalPages;
    private bool _isLoading;
    private const int SearchDelayMs = 500; // Задержка в миллисекундах перед выполнением поиска

    public ObservableCollection<MusicAlbumSearchResult> Results { get; } = new();

    public bool IsPreviousEnabled => !_isLoading && _currentPage > 1;
    public bool IsNextEnabled => !_isLoading && _totalPages > 0 && _currentPage < _totalPages;
    public string PageStatusText => GetPageStatusText();
    public string EmptyStateText => GetEmptyStateText();

    public AlbumSearchPage(MusicSearchService musicSearchService)
    {
        InitializeComponent();

        _musicSearchService = musicSearchService ?? throw new ArgumentNullException(nameof(musicSearchService));

        BindingContext = this;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        CancelDebounce();
        CancelSearch();
    }

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        var newQuery = e.NewTextValue?.Trim() ?? string.Empty;

        if (string.Equals(_currentQuery, newQuery, StringComparison.Ordinal))
        {
            return;
        }

        _currentQuery = newQuery;
        
        // Отменяем предыдущую задержку
        CancelDebounce();

        if (_currentQuery.Length < 3)
        {
            Results.Clear();
            _currentPage = 0;
            _totalPages = 0;
            UpdateUiState();
            return;
        }

        // Создаем новую задержку
        _debounceCts = new CancellationTokenSource();
        var debounceToken = _debounceCts.Token;

        try
        {
            // Ждем указанное время перед выполнением поиска
            await Task.Delay(SearchDelayMs, debounceToken);

            // Если задержка не была отменена, выполняем поиск
            if (!debounceToken.IsCancellationRequested)
            {
                CancelSearch();
                _searchCts = new CancellationTokenSource();

                try
                {
                    await SearchAsync(1, _searchCts.Token);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Задержка была отменена новым вводом текста - это нормально
        }
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
        CancelSearch();

        _searchCts = new CancellationTokenSource();

        try
        {
            await SearchAsync(page, _searchCts.Token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task SearchAsync(int page, CancellationToken cancellationToken)
    {
        if (page < 1)
        {
            page = 1;
        }

        _isLoading = true;
        UpdateUiState();

        try
        {
            var response = await _musicSearchService.SearchAlbumsAsync(_currentQuery, page, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            Results.Clear();
            foreach (var album in response.Results)
            {
                Results.Add(album);
            }

            _totalPages = response.TotalPages;
            if (_totalPages == 0 && Results.Count > 0)
            {
                _totalPages = 1;
            }

            _currentPage = Results.Count > 0 ? page : (_totalPages > 0 ? Math.Min(page, _totalPages) : 0);
        }
        finally
        {
            _isLoading = false;
            UpdateUiState();
        }
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

    private void CancelSearch()
    {
        if (_searchCts is null)
        {
            return;
        }

        if (!_searchCts.IsCancellationRequested)
        {
            _searchCts.Cancel();
        }

        _searchCts.Dispose();
        _searchCts = null;
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
        if (_isLoading)
        {
            return "Выполняется поиск...";
        }

        if (_currentQuery.Length < 3)
        {
            return "Введите минимум 3 символа";
        }

        if (_totalPages == 0)
        {
            return "Результаты не найдены";
        }

        return $"Страница {_currentPage} из {_totalPages}";
    }

    private string GetEmptyStateText()
    {
        if (_isLoading)
        {
            return "Выполняется поиск...";
        }

        if (_currentQuery.Length < 3)
        {
            return "Введите минимум 3 символа";
        }

        if (Results.Count == 0)
        {
            return "Альбомы не найдены";
        }

        return string.Empty;
    }
}

