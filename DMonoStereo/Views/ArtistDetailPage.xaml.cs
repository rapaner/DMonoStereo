using System.Collections.ObjectModel;
using DMonoStereo.Core.Models;
using DMonoStereo.Services;
using DMonoStereo.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DMonoStereo.Views;

public partial class ArtistDetailPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<Task> _onChanged;
    private readonly int _artistId;

    private Artist? _artist;

    public ObservableCollection<AlbumViewModel> Albums { get; } = new();

    public ArtistDetailPage(MusicService musicService, IServiceProvider serviceProvider, int artistId, Func<Task> onChanged)
    {
        InitializeComponent();

        _musicService = musicService;
        _serviceProvider = serviceProvider;
        _artistId = artistId;
        _onChanged = onChanged;

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadArtistAsync();
    }

    private async Task LoadArtistAsync()
    {
        _artist = await _musicService.GetArtistByIdAsync(_artistId);
        if (_artist == null)
        {
            await DisplayAlert("Ошибка", "Исполнитель не найден", "OK");
            await Navigation.PopAsync();
            return;
        }

        ArtistNameLabel.Text = _artist.Name;

        Albums.Clear();
        foreach (var album in _artist.Albums.OrderByDescending(a => a.DateAdded))
        {
            Albums.Add(AlbumViewModel.FromAlbum(album));
        }
    }

    private async void OnAddAlbumClicked(object? sender, EventArgs e)
    {
        if (_artist == null)
        {
            return;
        }

        var page = ActivatorUtilities.CreateInstance<AddEditAlbumPage>(
            _serviceProvider,
            _artist,
            new Func<Task>(LoadArtistAsync));

        await Navigation.PushAsync(page);
    }

    private async void OnEditArtistClicked(object? sender, EventArgs e)
    {
        if (_artist == null)
        {
            return;
        }

        var page = ActivatorUtilities.CreateInstance<AddEditArtistPage>(
            _serviceProvider,
            new Func<Task>(async () =>
            {
                await LoadArtistAsync();
                await _onChanged();
            }),
            _artist);

        await Navigation.PushAsync(page);
    }

    private async void OnDeleteArtistClicked(object? sender, EventArgs e)
    {
        if (_artist == null)
        {
            return;
        }

        var confirm = await DisplayAlert(
            "Удаление",
            $"Удалить исполнителя {_artist.Name}? Все альбомы и треки также будут удалены.",
            "Удалить",
            "Отмена");

        if (!confirm)
        {
            return;
        }

        await _musicService.DeleteArtistAsync(_artist.Id);
        await _onChanged();
        await Navigation.PopAsync();
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
            new Func<Task>(LoadArtistAsync));

        await Navigation.PushAsync(page);
    }
}
