using DMonoStereo.Core.Models;
using DMonoStereo.Helpers;
using DMonoStereo.Services;

namespace DMonoStereo.Views;

public partial class AddEditTrackPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly Func<Task> _onSaved;
    private readonly Album _album;
    private readonly Track? _track;

    public bool IsEditMode => _track != null;

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

    public AddEditTrackPage(MusicService musicService, Album album, Func<Task> onSaved, Track? track = null)
    {
        InitializeComponent();

        _musicService = musicService;
        _album = album;
        _onSaved = onSaved;
        _track = track;

        BindingContext = this;

        Title = track == null ? "Новый трек" : "Редактирование трека";

        if (track != null)
        {
            NameEntry.Text = track.Name;
            TrackNumberEntry.Text = track.TrackNumber?.ToString();
            DurationEntry.Text = TimeSpanHelpers.FormatDuration(track.Duration);
            RatingPicker.SelectedIndex = track.Rating.HasValue ? track.Rating.Value : 0;
        }
        else
        {
            RatingPicker.SelectedIndex = 0;
        }
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (_track == null)
        {
            return;
        }

        var confirm = await DisplayAlertAsync("Подтверждение", "Удалить трек?", "Удалить", "Отмена");
        if (!confirm)
        {
            return;
        }

        try
        {
            await _musicService.DeleteTrackAsync(_track.Id);
            await _onSaved();
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", $"Не удалось удалить трек: {ex.Message}", "OK");
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlertAsync("Ошибка", "Введите название трека", "OK");
            return;
        }

        int? trackNumber = null;
        if (!string.IsNullOrWhiteSpace(TrackNumberEntry.Text))
        {
            if (int.TryParse(TrackNumberEntry.Text, out var parsedNumber) && parsedNumber > 0)
            {
                trackNumber = parsedNumber;
            }
            else
            {
                await DisplayAlertAsync("Ошибка", "Введите корректный номер трека", "OK");
                return;
            }
        }

        if (!TimeSpanHelpers.TryParseDuration(DurationEntry.Text, out var durationSeconds))
        {
            await DisplayAlertAsync("Ошибка", "Введите длительность в формате мм:сс", "OK");
            return;
        }

        int? rating = null;
        if (RatingPicker.SelectedIndex > 0)
        {
            rating = RatingPicker.SelectedIndex;
        }

        try
        {
            if (_track == null)
            {
                var track = new Track
                {
                    Name = name,
                    TrackNumber = trackNumber,
                    Duration = durationSeconds,
                    Rating = rating,
                    AlbumId = _album.Id
                };

                await _musicService.AddTrackAsync(track);
            }
            else
            {
                _track.Name = name;
                _track.TrackNumber = trackNumber;
                _track.Duration = durationSeconds;
                _track.Rating = rating;

                await _musicService.UpdateTrackAsync(_track);
            }

            await _onSaved();
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", $"Не удалось сохранить трек: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}