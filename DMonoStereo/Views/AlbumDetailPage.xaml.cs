using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DMonoStereo.Core.Models;
using DMonoStereo.Services;
using DMonoStereo.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DMonoStereo.Views;

public partial class AlbumDetailPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<Task> _onChanged;
    private readonly int _albumId;

    private Album? _album;

    public ObservableCollection<TrackViewModel> Tracks { get; } = new();

    public AlbumDetailPage(MusicService musicService, IServiceProvider serviceProvider, int albumId, Func<Task> onChanged)
    {
        InitializeComponent();

        _musicService = musicService;
        _serviceProvider = serviceProvider;
        _albumId = albumId;
        _onChanged = onChanged;

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAlbumAsync();
    }

    private async Task LoadAlbumAsync()
    {
        _album = await _musicService.GetAlbumByIdAsync(_albumId);
        if (_album == null)
        {
            await DisplayAlert("Ошибка", "Альбом не найден", "OK");
            await Navigation.PopAsync();
            return;
        }

        AlbumNameLabel.Text = _album.Name;
        ArtistNameLabel.Text = _album.Artist?.Name ?? string.Empty;
        YearLabel.Text = _album.Year.HasValue ? $"Год: {_album.Year}" : string.Empty;
        YearLabel.IsVisible = _album.Year.HasValue;
        RatingLabel.Text = _album.Rating.HasValue ? $"Рейтинг: ★ {_album.Rating}" : "Рейтинг: —";
        TrackCountLabel.Text = $"Треков: {_album.Tracks.Count}";

        if (_album.CoverImage != null && _album.CoverImage.Length > 0)
        {
            CoverImage.Source = ImageSource.FromStream(() => new MemoryStream(_album.CoverImage));
        }
        else
        {
            CoverImage.Source = null;
        }

        Tracks.Clear();
        foreach (var track in _album.Tracks.OrderBy(t => t.TrackNumber ?? int.MaxValue).ThenBy(t => t.Name))
        {
            Tracks.Add(TrackViewModel.FromTrack(track));
        }
    }

    private async void OnEditAlbumClicked(object? sender, EventArgs e)
    {
        if (_album == null || _album.Artist == null)
        {
            return;
        }

        var page = ActivatorUtilities.CreateInstance<AddEditAlbumPage>(
            _serviceProvider,
            _album.Artist,
            new Func<Task>(async () =>
            {
                await LoadAlbumAsync();
                await _onChanged();
            }),
            _album);

        await Navigation.PushAsync(page);
    }

    private async void OnDeleteAlbumClicked(object? sender, EventArgs e)
    {
        if (_album == null)
        {
            return;
        }

        var confirm = await DisplayAlert("Удаление", $"Удалить альбом {_album.Name}?", "Удалить", "Отмена");
        if (!confirm)
        {
            return;
        }

        await _musicService.DeleteAlbumAsync(_album.Id);
        await _onChanged();
        await Navigation.PopAsync();
    }

    private async void OnAddTrackClicked(object? sender, EventArgs e)
    {
        if (_album == null)
        {
            return;
        }

        var page = ActivatorUtilities.CreateInstance<AddEditTrackPage>(
            _serviceProvider,
            _album,
            new Func<Task>(LoadAlbumAsync));

        await Navigation.PushAsync(page);
    }

    private async void OnTrackSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }

        if (_album == null)
        {
            return;
        }

        if (e.CurrentSelection.FirstOrDefault() is not TrackViewModel trackViewModel)
        {
            return;
        }

        var track = _album.Tracks.FirstOrDefault(t => t.Id == trackViewModel.Id);
        if (track == null)
        {
            return;
        }

        var page = ActivatorUtilities.CreateInstance<AddEditTrackPage>(
            _serviceProvider,
            _album,
            new Func<Task>(LoadAlbumAsync),
            track);

        await Navigation.PushAsync(page);
    }
}
