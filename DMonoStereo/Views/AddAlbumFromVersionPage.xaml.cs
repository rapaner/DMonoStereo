using DMonoStereo.Models;
using DMonoStereo.Services;
using DMonoStereo.ViewModels;

namespace DMonoStereo.Views;

public partial class AddAlbumFromVersionPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly AddAlbumFromVersionViewModel _viewModel;
    private CancellationTokenSource? _loadCts;
    private bool _isSaving;

    public AddAlbumFromVersionPage(
        MusicSearchService musicSearchService,
        MusicService musicService,
        MusicAlbumVersionSummary version)
    {
        InitializeComponent();

        if (musicSearchService is null)
        {
            throw new ArgumentNullException(nameof(musicSearchService));
        }

        _musicService = musicService ?? throw new ArgumentNullException(nameof(musicService));

        if (version is null)
        {
            throw new ArgumentNullException(nameof(version));
        }

        _viewModel = new AddAlbumFromVersionViewModel(musicSearchService, version);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel.HasLoaded || _viewModel.IsLoading)
        {
            return;
        }

        _loadCts = new CancellationTokenSource();

        try
        {
            await _viewModel.LoadAsync(_loadCts.Token);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить данные об альбоме: {ex.Message}", "OK");
            await Navigation.PopAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        CancelLoading();
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

    private async void OnAddAlbumClicked(object? sender, EventArgs e)
    {
        if (_isSaving)
        {
            return;
        }

        var artistName = _viewModel.ArtistName?.Trim();
        if (string.IsNullOrWhiteSpace(artistName))
        {
            await DisplayAlert("Ошибка", "Введите имя исполнителя", "OK");
            return;
        }

        var albumTitle = _viewModel.AlbumTitle?.Trim();
        if (string.IsNullOrWhiteSpace(albumTitle))
        {
            await DisplayAlert("Ошибка", "Введите название альбома", "OK");
            return;
        }

        int? year = null;
        if (!string.IsNullOrWhiteSpace(_viewModel.Year))
        {
            if (int.TryParse(_viewModel.Year, out var parsedYear))
            {
                year = parsedYear;
            }
            else
            {
                await DisplayAlert("Ошибка", "Введите корректный год", "OK");
                return;
            }
        }

        var selectedTracks = _viewModel.BuildSelectedTracksWithRenumbering();

        if (selectedTracks.Count == 0)
        {
            await DisplayAlert("Ошибка", "Выберите хотя бы один трек", "OK");
            return;
        }

        try
        {
            _isSaving = true;

            await _musicService.AddAlbumFromSearchAsync(
                artistName,
                albumTitle,
                year,
                _viewModel.CoverImageData,
                _viewModel.ArtistImageData,
                selectedTracks);

            await DisplayAlert("Успех", "Альбом успешно добавлен", "OK");
            await Navigation.PopAsync();
        }
        catch (InvalidOperationException ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось добавить альбом: {ex.Message}", "OK");
        }
        finally
        {
            _isSaving = false;
        }
    }
}