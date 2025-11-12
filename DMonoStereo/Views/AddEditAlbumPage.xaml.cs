using DMonoStereo.Core.Models;
using DMonoStereo.Services;
using System.Collections.ObjectModel;

namespace DMonoStereo.Views;

public partial class AddEditAlbumPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly ImageService _imageService;
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<Task> _onSaved;
    private readonly Album? _album;

    private readonly List<Artist> _artists = new();
    private readonly ObservableCollection<ArtistSuggestionItem> _suggestions = new();

    private byte[]? _coverImage;
    private Artist? _selectedArtist;
    private bool _isInitialized;
    private bool _suppressSearchUpdates;

    public List<string> RatingOptions { get; } = new()
    {
        "Без оценки",
        "1",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "9",
        "10"
    };

    public AddEditAlbumPage(
        MusicService musicService,
        ImageService imageService,
        IServiceProvider serviceProvider,
        Func<Task> onSaved,
        Artist? artist = null,
        Album? album = null)
    {
        InitializeComponent();

        _musicService = musicService;
        _imageService = imageService;
        _serviceProvider = serviceProvider;
        _onSaved = onSaved;
        _album = album;
        _selectedArtist = artist ?? album?.Artist;

        BindingContext = this;

        ArtistSuggestionsView.ItemsSource = _suggestions;
        UpdateSelectedArtistUI();
        SetArtistSearchText(_selectedArtist?.Name, triggerSuggestions: false);
        UpdateEmptyState();

        Title = album == null ? "Новый альбом" : "Редактирование альбома";

        if (album != null)
        {
            NameEntry.Text = album.Name;
            YearEntry.Text = album.Year?.ToString();
            _coverImage = album.CoverImage;
            UpdateCoverPreview();

            if (album.Rating.HasValue)
            {
                RatingPicker.SelectedIndex = album.Rating.Value;
            }
            else
            {
                RatingPicker.SelectedIndex = 0;
            }
        }
        else
        {
            RatingPicker.SelectedIndex = 0;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isInitialized)
        {
            await LoadArtistsAsync();
            _isInitialized = true;
        }
    }

    private async Task LoadArtistsAsync()
    {
        var artists = await _musicService.GetArtistsAsync();

        _artists.Clear();
        _artists.AddRange(artists);

        if (_selectedArtist != null)
        {
            var match = _artists.FirstOrDefault(a => a.Id == _selectedArtist.Id);
            _selectedArtist = match;
            SetArtistSearchText(_selectedArtist?.Name, triggerSuggestions: false);
        }

        UpdateSelectedArtistUI();
        RefreshSuggestions(ArtistSearchBar.Text);
        UpdateEmptyState(!string.IsNullOrWhiteSpace(ArtistSearchBar.Text));
    }

    private void RefreshSuggestions(string? searchText)
    {
        if (_suppressSearchUpdates)
        {
            return;
        }

        _suggestions.Clear();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            ArtistSuggestionsView.IsVisible = false;
            UpdateEmptyState();
            return;
        }

        var normalized = searchText.Trim();
        if (normalized.Length == 0)
        {
            ArtistSuggestionsView.IsVisible = false;
            UpdateEmptyState();
            return;
        }

        var nameCounts = _artists
            .GroupBy(a => a.Name)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var artist in _artists
                     .Where(a => a.Name.Contains(normalized, StringComparison.OrdinalIgnoreCase))
                     .OrderBy(a => a.Name)
                     .Take(20))
        {
            var displayName = nameCounts.TryGetValue(artist.Name, out var count) && count > 1
                ? $"{artist.Name} (#{artist.Id})"
                : artist.Name;

            _suggestions.Add(new ArtistSuggestionItem(artist, displayName));
        }

        ArtistSuggestionsView.IsVisible = _suggestions.Count > 0;
        UpdateEmptyState(true);
    }

    private void UpdateEmptyState(bool hasSearch = false)
    {
        if (_artists.Count == 0)
        {
            NoArtistsLabel.IsVisible = true;
        }
        else if (hasSearch)
        {
            NoArtistsLabel.IsVisible = _suggestions.Count == 0;
        }
        else
        {
            NoArtistsLabel.IsVisible = false;
        }
    }

    private void UpdateSelectedArtistUI()
    {
        if (_selectedArtist != null)
        {
            SelectedArtistFrame.IsVisible = true;
            SelectedArtistLabel.Text = _selectedArtist.Name;
        }
        else
        {
            SelectedArtistFrame.IsVisible = false;
            SelectedArtistLabel.Text = string.Empty;
        }
    }

    private void SetArtistSearchText(string? text, bool triggerSuggestions = true)
    {
        _suppressSearchUpdates = true;
        ArtistSearchBar.Text = text ?? string.Empty;
        _suppressSearchUpdates = false;

        if (triggerSuggestions)
        {
            RefreshSuggestions(ArtistSearchBar.Text);
        }
    }

    private void SelectArtist(Artist artist)
    {
        _selectedArtist = artist;
        SetArtistSearchText(artist.Name, triggerSuggestions: false);
        ArtistSuggestionsView.IsVisible = false;
        if (ArtistSuggestionsView.SelectedItem != null)
        {
            ArtistSuggestionsView.SelectedItem = null;
        }
        UpdateSelectedArtistUI();
    }

    private async Task OnArtistSavedAsync()
    {
        var previousSearch = ArtistSearchBar.Text;

        await LoadArtistsAsync();

        if (!string.IsNullOrWhiteSpace(previousSearch))
        {
            var match = _artists.FirstOrDefault(a => a.Name.Equals(previousSearch, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                SelectArtist(match);
                return;
            }
        }

        var latest = _artists.OrderByDescending(a => a.DateAdded).FirstOrDefault();
        if (latest != null)
        {
            SelectArtist(latest);
        }
    }

    private async void OnPickCoverClicked(object? sender, EventArgs e)
    {
        try
        {
            var imageBytes = await _imageService.PickAndResizeImageAsync();
            if (imageBytes != null)
            {
                _coverImage = imageBytes;
                UpdateCoverPreview();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", $"Не удалось выбрать изображение: {ex.Message}", "OK");
        }
    }

    private void OnRemoveCoverClicked(object? sender, EventArgs e)
    {
        _coverImage = null;
        UpdateCoverPreview();
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_selectedArtist == null)
        {
            await DisplayAlertAsync("Ошибка", "Выберите исполнителя для альбома", "OK");
            return;
        }

        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlertAsync("Ошибка", "Введите название альбома", "OK");
            return;
        }

        int? year = null;
        if (!string.IsNullOrWhiteSpace(YearEntry.Text))
        {
            if (int.TryParse(YearEntry.Text, out var parsedYear))
            {
                year = parsedYear;
            }
            else
            {
                await DisplayAlertAsync("Ошибка", "Введите корректный год", "OK");
                return;
            }
        }

        int? rating = null;
        if (RatingPicker.SelectedIndex > 0)
        {
            rating = RatingPicker.SelectedIndex;
        }

        if (await _musicService.AlbumExistsForArtistAsync(_selectedArtist.Id, name, _album?.Id))
        {
            await DisplayAlertAsync("Ошибка", "У этого исполнителя уже есть альбом с таким названием", "OK");
            return;
        }

        try
        {
            if (_album == null)
            {
                var album = new Album
                {
                    Name = name,
                    Year = year,
                    Rating = rating,
                    CoverImage = _coverImage,
                    ArtistId = _selectedArtist.Id
                };

                await _musicService.AddAlbumAsync(album);
            }
            else
            {
                _album.Name = name;
                _album.Year = year;
                _album.Rating = rating;
                _album.CoverImage = _coverImage;
                _album.ArtistId = _selectedArtist.Id;
                _album.Artist = _selectedArtist;

                await _musicService.UpdateAlbumAsync(_album);
            }

            await _onSaved();
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", $"Не удалось сохранить альбом: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnArtistSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_suppressSearchUpdates)
        {
            return;
        }

        RefreshSuggestions(e.NewTextValue);
    }

    private void OnArtistSearchButtonPressed(object? sender, EventArgs e)
    {
        if (_suggestions.Count > 0)
        {
            SelectArtist(_suggestions[0].Artist);
        }
        else if (!string.IsNullOrWhiteSpace(ArtistSearchBar.Text))
        {
            var match = _artists.FirstOrDefault(a =>
                a.Name.Equals(ArtistSearchBar.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                SelectArtist(match);
            }
        }
    }

    private void OnArtistSuggestionSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is ArtistSuggestionItem item)
        {
            SelectArtist(item.Artist);
        }

        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }
    }

    private async void OnAddArtistClicked(object? sender, EventArgs e)
    {
        var page = ActivatorUtilities.CreateInstance<AddEditArtistPage>(
            _serviceProvider,
            new Func<Task>(OnArtistSavedAsync));

        await Navigation.PushAsync(page);
    }

    private void OnClearSelectedArtistClicked(object? sender, EventArgs e)
    {
        _selectedArtist = null;
        SetArtistSearchText(string.Empty);
        UpdateSelectedArtistUI();
    }

    private void UpdateCoverPreview()
    {
        if (_coverImage != null && _coverImage.Length > 0)
        {
            CoverImage.Source = ImageSource.FromStream(() => new MemoryStream(_coverImage));
            CoverImage.IsVisible = true;
            NoCoverLabel.IsVisible = false;
        }
        else
        {
            CoverImage.Source = null;
            CoverImage.IsVisible = false;
            NoCoverLabel.IsVisible = true;
        }
    }

    public sealed record ArtistSuggestionItem(Artist Artist, string DisplayName);
}