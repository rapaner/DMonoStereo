using System.Collections.ObjectModel;
using DMonoStereo.Services;
using DMonoStereo.ViewModels;
using DMonoStereo.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DMonoStereo;

public partial class MainPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly IServiceProvider _serviceProvider;
    private bool _isInitialized;

    public ObservableCollection<ArtistViewModel> Artists { get; } = new();

    public MainPage(MusicService musicService, IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _musicService = musicService;
        _serviceProvider = serviceProvider;

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isInitialized)
        {
            await _musicService.InitializeAsync();
            _isInitialized = true;
        }

        await LoadArtistsAsync();
    }

    private async Task LoadArtistsAsync()
    {
        var artists = await _musicService.GetArtistsAsync();

        Artists.Clear();
        foreach (var artist in artists)
        {
            Artists.Add(ArtistViewModel.FromArtist(artist));
        }
    }

    private async void OnAddArtistClicked(object? sender, EventArgs e)
    {
        var page = ActivatorUtilities.CreateInstance<AddEditArtistPage>(
            _serviceProvider,
            new Func<Task>(LoadArtistsAsync));

        await Navigation.PushAsync(page);
    }

    private async void OnArtistSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }

        if (e.CurrentSelection.FirstOrDefault() is not ArtistViewModel selectedArtist)
        {
            return;
        }

        var page = ActivatorUtilities.CreateInstance<ArtistDetailPage>(
            _serviceProvider,
            selectedArtist.Id,
            new Func<Task>(LoadArtistsAsync));

        await Navigation.PushAsync(page);
    }

    private async void OnOpenYandexDiskClicked(object? sender, EventArgs e)
    {
        var page = _serviceProvider.GetRequiredService<YandexDiskPage>();
        await Navigation.PushAsync(page);
    }
}
