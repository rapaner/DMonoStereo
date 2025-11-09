using System.Collections.Generic;
using System.IO;
using DMonoStereo.Core.Models;
using DMonoStereo.Services;

namespace DMonoStereo.Views;

public partial class AddEditAlbumPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly ImageService _imageService;
    private readonly Func<Task> _onSaved;
    private readonly Artist _artist;
    private readonly Album? _album;

    private byte[]? _coverImage;

    public List<string> RatingOptions { get; } = new() { "Без оценки", "1", "2", "3", "4", "5" };

    public AddEditAlbumPage(MusicService musicService, ImageService imageService, Artist artist, Func<Task> onSaved, Album? album = null)
    {
        InitializeComponent();

        _musicService = musicService;
        _imageService = imageService;
        _artist = artist;
        _onSaved = onSaved;
        _album = album;

        BindingContext = this;

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
            await DisplayAlert("Ошибка", $"Не удалось выбрать изображение: {ex.Message}", "OK");
        }
    }

    private void OnRemoveCoverClicked(object? sender, EventArgs e)
    {
        _coverImage = null;
        UpdateCoverPreview();
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlert("Ошибка", "Введите название альбома", "OK");
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
                await DisplayAlert("Ошибка", "Введите корректный год", "OK");
                return;
            }
        }

        int? rating = null;
        if (RatingPicker.SelectedIndex > 0)
        {
            rating = RatingPicker.SelectedIndex;
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
                    ArtistId = _artist.Id
                };

                await _musicService.AddAlbumAsync(album);
            }
            else
            {
                _album.Name = name;
                _album.Year = year;
                _album.Rating = rating;
                _album.CoverImage = _coverImage;

                await _musicService.UpdateAlbumAsync(_album);
            }

            await _onSaved();
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось сохранить альбом: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
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
}
