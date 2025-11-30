using DMonoStereo.Models;
using DMonoStereo.Services;
using DMonoStereo.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DMonoStereo.Views;

public partial class AddAlbumFromVersionPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly AddAlbumFromVersionViewModel _viewModel;
    private CancellationTokenSource? _loadCts;
    private bool _isSaving;
    private ObservableCollection<EditableTrackViewModel>? _subscribedTracks;

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

        WireTracksSubscriptions();
        UpdateToggleButtonText();
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
            await DisplayAlertAsync("Ошибка", $"Не удалось загрузить данные об альбоме: {ex.Message}", "OK");
            await Navigation.PopAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        CancelLoading();
        UnwireTracksSubscriptions();
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
            await DisplayAlertAsync("Ошибка", "Введите имя исполнителя", "OK");
            return;
        }

        var albumTitle = _viewModel.AlbumTitle?.Trim();
        if (string.IsNullOrWhiteSpace(albumTitle))
        {
            await DisplayAlertAsync("Ошибка", "Введите название альбома", "OK");
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
                await DisplayAlertAsync("Ошибка", "Введите корректный год", "OK");
                return;
            }
        }

        var selectedTracks = _viewModel.BuildSelectedTracksWithRenumbering();

        if (selectedTracks.Count == 0)
        {
            await DisplayAlertAsync("Ошибка", "Выберите хотя бы один трек", "OK");
            return;
        }

        // Проверяем существование альбома у исполнителя
        var existingArtist = await _musicService.GetArtistByNameAsync(artistName);
        if (existingArtist != null)
        {
            if (await _musicService.AlbumExistsForArtistAsync(existingArtist.Id, albumTitle))
            {
                await DisplayAlertAsync("Ошибка", "У этого исполнителя уже есть альбом с таким названием", "OK");
                return;
            }
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

            await DisplayAlertAsync("Успех", "Альбом успешно добавлен", "OK");
            await Navigation.PopAsync();
        }
        catch (InvalidOperationException ex)
        {
            await DisplayAlertAsync("Ошибка", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", $"Не удалось добавить альбом: {ex.Message}", "OK");
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void OnToggleSelectAllClicked(object? sender, EventArgs e)
    {
        var tracks = _viewModel.Tracks;
        if (tracks is null || tracks.Count == 0)
        {
            return;
        }

        var anySelected = tracks.Any(t => t.IsSelected);
        foreach (var t in tracks)
        {
            t.IsSelected = !anySelected;
        }

        UpdateToggleButtonText();
    }

    private void UpdateToggleButtonText()
    {
        if (ToggleSelectButton == null)
        {
            return;
        }

        var tracks = _viewModel.Tracks;
        var anySelected = tracks.Any(t => t.IsSelected);
        ToggleSelectButton.Text = anySelected ? "Снять отметки" : "Выбрать все";
    }

    private void WireTracksSubscriptions()
    {
        UnwireTracksSubscriptions();

        _subscribedTracks = _viewModel.Tracks;
        if (_subscribedTracks == null)
        {
            return;
        }

        _subscribedTracks.CollectionChanged += Tracks_CollectionChanged;
        foreach (var item in _subscribedTracks)
        {
            item.PropertyChanged += Track_PropertyChanged;
        }
    }

    private void UnwireTracksSubscriptions()
    {
        if (_subscribedTracks == null)
        {
            return;
        }

        _subscribedTracks.CollectionChanged -= Tracks_CollectionChanged;
        foreach (var item in _subscribedTracks)
        {
            item.PropertyChanged -= Track_PropertyChanged;
        }

        _subscribedTracks = null;
    }

    private void Tracks_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (var obj in e.OldItems)
            {
                if (obj is EditableTrackViewModel oldItem)
                {
                    oldItem.PropertyChanged -= Track_PropertyChanged;
                }
            }
        }

        if (e.NewItems != null)
        {
            foreach (var obj in e.NewItems)
            {
                if (obj is EditableTrackViewModel newItem)
                {
                    newItem.PropertyChanged += Track_PropertyChanged;
                }
            }
        }

        UpdateToggleButtonText();
    }

    private void Track_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditableTrackViewModel.IsSelected))
        {
            UpdateToggleButtonText();
        }
    }
}