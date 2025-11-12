using DMonoStereo.Core.Models;
using DMonoStereo.Services;

namespace DMonoStereo.Views;

public partial class AddEditArtistPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly ImageService _imageService;
    private readonly Func<Task> _onSaved;
    private readonly Artist? _artist;

    private byte[]? _coverImage;

    public AddEditArtistPage(MusicService musicService, ImageService imageService, Func<Task> onSaved, Artist? artist = null)
    {
        InitializeComponent();

        _musicService = musicService;
        _imageService = imageService;
        _onSaved = onSaved;
        _artist = artist;

        if (_artist != null)
        {
            Title = "Редактирование исполнителя";
            NameEntry.Text = _artist.Name;
            _coverImage = _artist.CoverImage;
        }
        else
        {
            Title = "Новый исполнитель";
        }

        UpdateCoverPreview();
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlertAsync("Ошибка", "Введите имя исполнителя", "OK");
            return;
        }

        try
        {
            var excludeArtistId = _artist?.Id;
            if (await _musicService.ArtistExistsByNameAsync(name, excludeArtistId))
            {
                await DisplayAlertAsync("Ошибка", "Исполнитель с таким именем уже существует", "OK");
                return;
            }

            if (_artist == null)
            {
                var artist = new Artist
                {
                    Name = name,
                    CoverImage = _coverImage
                };

                await _musicService.AddArtistAsync(artist);
            }
            else
            {
                _artist.Name = name;
                _artist.CoverImage = _coverImage;
                await _musicService.UpdateArtistAsync(_artist);
            }

            await _onSaved();
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", $"Не удалось сохранить исполнителя: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
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