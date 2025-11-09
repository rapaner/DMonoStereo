using System;
using System.Collections.Generic;
using System.Globalization;
using DMonoStereo.Core.Models;
using DMonoStereo.Services;

namespace DMonoStereo.Views;

public partial class AddEditTrackPage : ContentPage
{
    private readonly MusicService _musicService;
    private readonly Func<Task> _onSaved;
    private readonly Album _album;
    private readonly Track? _track;

    public List<string> RatingOptions { get; } = new() { "Без оценки", "1", "2", "3", "4", "5" };

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
            DurationEntry.Text = FormatDuration(track.Duration);
            RatingPicker.SelectedIndex = track.Rating.HasValue ? track.Rating.Value : 0;
        }
        else
        {
            RatingPicker.SelectedIndex = 0;
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlert("Ошибка", "Введите название трека", "OK");
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
                await DisplayAlert("Ошибка", "Введите корректный номер трека", "OK");
                return;
            }
        }

        if (!TryParseDuration(DurationEntry.Text, out var durationSeconds))
        {
            await DisplayAlert("Ошибка", "Введите длительность в формате мм:сс", "OK");
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
            await DisplayAlert("Ошибка", $"Не удалось сохранить трек: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private static bool TryParseDuration(string? text, out int seconds)
    {
        seconds = 0;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var normalized = text.Trim();
        if (TimeSpan.TryParseExact(normalized, @"m\:ss", CultureInfo.InvariantCulture, out var time) ||
            TimeSpan.TryParseExact(normalized, @"mm\:ss", CultureInfo.InvariantCulture, out time) ||
            TimeSpan.TryParseExact(normalized, @"h\:mm\:ss", CultureInfo.InvariantCulture, out time))
        {
            seconds = (int)Math.Round(time.TotalSeconds);
            return seconds > 0;
        }

        if (int.TryParse(normalized, out var rawSeconds) && rawSeconds > 0)
        {
            seconds = rawSeconds;
            return true;
        }

        return false;
    }

    private static string FormatDuration(int seconds)
    {
        var time = TimeSpan.FromSeconds(seconds);
        return time.Hours > 0 ? time.ToString(@"h\:mm\:ss") : time.ToString(@"mm\:ss");
    }
}
