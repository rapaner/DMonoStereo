using DMonoStereo.Services;
using DMonoStereo.ViewModels;
using System.Collections.ObjectModel;

namespace DMonoStereo.Views;

public partial class AllAlbumsPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly IServiceProvider _serviceProvider;

    public ObservableCollection<AlbumViewModel> Albums { get; } = new();

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
        await LoadAlbumsAsync();
    }

    private async Task LoadAlbumsAsync()
    {
        var albums = await _musicService.GetAllAlbumsAsync();

        Albums.Clear();
        foreach (var album in albums)
        {
            Albums.Add(AlbumViewModel.FromAlbum(album));
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
            new Func<Task>(LoadAlbumsAsync));

        await Navigation.PushAsync(page);
    }
}