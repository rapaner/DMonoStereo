using DMonoStereo.Core.Models;
using DMonoStereo.Services;

namespace DMonoStereo.Views;

public partial class AddEditArtistPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly Func<Task> _onSaved;
    private readonly Artist? _artist;

    public AddEditArtistPage(MusicService musicService, Func<Task> onSaved, Artist? artist = null)
    {
        InitializeComponent();

        _musicService = musicService;
        _onSaved = onSaved;
        _artist = artist;

        if (_artist != null)
        {
            Title = "Редактирование исполнителя";
            NameEntry.Text = _artist.Name;
        }
        else
        {
            Title = "Новый исполнитель";
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlert("Ошибка", "Введите имя исполнителя", "OK");
            return;
        }

        try
        {
            if (_artist == null)
            {
                var artist = new Artist
                {
                    Name = name
                };

                await _musicService.AddArtistAsync(artist);
            }
            else
            {
                _artist.Name = name;
                await _musicService.UpdateArtistAsync(_artist);
            }

            await _onSaved();
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось сохранить исполнителя: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
