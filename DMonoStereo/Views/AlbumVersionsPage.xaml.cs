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
    private bool _areFiltersVisible;
    private MusicFilterOption? _selectedFormat;
    private MusicFilterOption? _selectedCountry;
    private MusicFilterOption? _selectedYear;
    private bool _isApplyingFilters;

    public ObservableCollection<MusicAlbumVersionSummary> Versions { get; } = new();
    public ObservableCollection<MusicFilterOption> FormatFilters { get; } = new();
    public ObservableCollection<MusicFilterOption> CountryFilters { get; } = new();
    public ObservableCollection<MusicFilterOption> YearFilters { get; } = new();

    public bool HasFormatFilters => FormatFilters.Count > 0;
    public bool HasCountryFilters => CountryFilters.Count > 0;
    public bool HasYearFilters => YearFilters.Count > 0;

    public string AlbumTitle => _album.Title ?? "Версии альбома";
    public bool IsFirstEnabled => !_isLoading && _currentPage > 1;
    public bool IsPreviousEnabled => !_isLoading && _currentPage > 1;
    public bool IsNextEnabled => !_isLoading && _totalPages > 0 && _currentPage < _totalPages;
    public bool IsLastEnabled => !_isLoading && _totalPages > 0 && _currentPage < _totalPages;
    public string PageStatusText => GetPageStatusText();
    public string EmptyStateText => GetEmptyStateText();
    public bool AreFiltersVisible
    {
        get => _areFiltersVisible;
        set
        {
            if (_areFiltersVisible == value) return;
            _areFiltersVisible = value;
            OnPropertyChanged();
        }
    }
    public MusicFilterOption? SelectedFormat
    {
        get => _selectedFormat;
        set
        {
            if (_selectedFormat == value) return;
            _selectedFormat = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AppliedFiltersText));
            OnPropertyChanged(nameof(HasActiveFilters));
            if (!_isApplyingFilters)
            {
                _ = ApplyFiltersAsync();
            }
        }
    }
    public MusicFilterOption? SelectedCountry
    {
        get => _selectedCountry;
        set
        {
            if (_selectedCountry == value) return;
            _selectedCountry = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AppliedFiltersText));
            OnPropertyChanged(nameof(HasActiveFilters));
            if (!_isApplyingFilters)
            {
                _ = ApplyFiltersAsync();
            }
        }
    }
    public MusicFilterOption? SelectedYear
    {
        get => _selectedYear;
        set
        {
            if (_selectedYear == value) return;
            _selectedYear = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AppliedFiltersText));
            OnPropertyChanged(nameof(HasActiveFilters));
            if (!_isApplyingFilters)
            {
                _ = ApplyFiltersAsync();
            }
        }
    }
    public string AppliedFiltersText => GetAppliedFiltersText();
    public bool HasActiveFilters => _selectedFormat is not null || _selectedCountry is not null || _selectedYear is not null;

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
            var formatValue = _selectedFormat?.Value;
            var countryValue = _selectedCountry?.Value;
            int? yearValue = null;
            if (_selectedYear is not null && int.TryParse(_selectedYear.Value, out var year))
            {
                yearValue = year;
            }

            var response = await _musicSearchService.GetAlbumVersionsAsync(
                _album,
                page,
                formatValue,
                countryValue,
                yearValue,
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            Versions.Clear();
            foreach (var version in response.Versions)
            {
                Versions.Add(version);
            }

            UpdateFilterLists(response);

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

    private void UpdateFilterLists(MusicAlbumVersionsResponse response)
    {
        FormatFilters.Clear();
        foreach (var filter in response.FormatFilters)
        {
            FormatFilters.Add(filter);
        }

        CountryFilters.Clear();
        foreach (var filter in response.CountryFilters)
        {
            CountryFilters.Add(filter);
        }

        YearFilters.Clear();
        foreach (var filter in response.YearFilters)
        {
            YearFilters.Add(filter);
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            OnPropertyChanged(nameof(FormatFilters));
            OnPropertyChanged(nameof(CountryFilters));
            OnPropertyChanged(nameof(YearFilters));
            OnPropertyChanged(nameof(HasFormatFilters));
            OnPropertyChanged(nameof(HasCountryFilters));
            OnPropertyChanged(nameof(HasYearFilters));
        });
    }

    private void OnFiltersToggleClicked(object? sender, EventArgs e)
    {
        AreFiltersVisible = !AreFiltersVisible;
    }

    private void OnFilterSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Событие используется только для предотвращения автоматического сброса выбранного элемента
        // Логика применения фильтров выполняется через сеттеры свойств
    }

    private async Task ApplyFiltersAsync()
    {
        if (_isApplyingFilters)
        {
            return;
        }

        _isApplyingFilters = true;
        try
        {
            await NavigateToPageAsync(1);
        }
        finally
        {
            _isApplyingFilters = false;
        }
    }

    private async void OnResetFiltersClicked(object? sender, EventArgs e)
    {
        ResetFilters();
        await NavigateToPageAsync(1);
    }

    private void ResetFilters()
    {
        _isApplyingFilters = true;
        try
        {
            _selectedFormat = null;
            _selectedCountry = null;
            _selectedYear = null;
            OnPropertyChanged(nameof(SelectedFormat));
            OnPropertyChanged(nameof(SelectedCountry));
            OnPropertyChanged(nameof(SelectedYear));
            OnPropertyChanged(nameof(AppliedFiltersText));
            OnPropertyChanged(nameof(HasActiveFilters));
        }
        finally
        {
            _isApplyingFilters = false;
        }
    }

    private string GetAppliedFiltersText()
    {
        var filters = new List<string>();
        if (_selectedFormat is not null)
        {
            filters.Add($"Формат: {_selectedFormat.Title}");
        }
        if (_selectedCountry is not null)
        {
            filters.Add($"Страна: {_selectedCountry.Title}");
        }
        if (_selectedYear is not null)
        {
            filters.Add($"Год: {_selectedYear.Title}");
        }
        return filters.Count > 0 ? string.Join(", ", filters) : string.Empty;
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
            OnPropertyChanged(nameof(AppliedFiltersText));
            OnPropertyChanged(nameof(HasActiveFilters));
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