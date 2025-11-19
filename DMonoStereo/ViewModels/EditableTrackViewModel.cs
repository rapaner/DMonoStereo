using DMonoStereo.Core.Models;
using DMonoStereo.Helpers;
using DMonoStereo.Models;
using System.ComponentModel;

namespace DMonoStereo.ViewModels;

/// <summary>
/// ViewModel для редактируемого трека при добавлении альбома из поиска.
/// </summary>
public class EditableTrackViewModel : INotifyPropertyChanged
{
    private string _title;
    private string _duration;
    private bool _isSelected;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Позиция трека в альбоме.
    /// </summary>
    public int Position { get; init; }

    /// <summary>
    /// Название трека (редактируемое).
    /// </summary>
    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
    }

    /// <summary>
    /// Длительность трека в формате "мм:сс" (редактируемое).
    /// </summary>
    public string Duration
    {
        get => _duration;
        set
        {
            if (_duration != value)
            {
                _duration = value;
                OnPropertyChanged(nameof(Duration));
            }
        }
    }

    /// <summary>
    /// Флаг выбора трека для добавления в альбом.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }

    /// <summary>
    /// Создает EditableTrackViewModel из MusicAlbumDetailTrack.
    /// </summary>
    public static EditableTrackViewModel FromMusicAlbumDetailTrack(MusicAlbumDetailTrack track)
    {
        return FromTrackInfo(track.Position, track.Title, track.Duration);
    }

    /// <summary>
    /// Создает EditableTrackViewModel из MusicAlbumVersionTrack.
    /// </summary>
    public static EditableTrackViewModel FromMusicAlbumVersionTrack(MusicAlbumVersionTrack track)
    {
        return FromTrackInfo(track.Position, track.Title, track.Duration);
    }

    private static EditableTrackViewModel FromTrackInfo(int position, string? title, string? duration)
    {
        var durationText = duration ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(durationText) &&
            int.TryParse(durationText, out var seconds))
        {
            durationText = TimeSpanHelpers.FormatDuration(seconds);
        }

        return new EditableTrackViewModel
        {
            Position = position,
            Title = title ?? string.Empty,
            Duration = durationText,
            IsSelected = true
        };
    }

    /// <summary>
    /// Преобразует EditableTrackViewModel в Track для сохранения в БД.
    /// </summary>
    /// <param name="albumId">Идентификатор альбома.</param>
    /// <returns>Экземпляр Track или null, если данные невалидны.</returns>
    public Track? ToTrack(int albumId)
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            return null;
        }

        if (!TimeSpanHelpers.TryParseDuration(Duration, out var durationSeconds))
        {
            return null;
        }

        return new Track
        {
            Name = Title.Trim(),
            Duration = durationSeconds,
            TrackNumber = Position,
            AlbumId = albumId
        };
    }

    /// <summary>
    /// Проверяет валидность данных трека.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Title) &&
               TimeSpanHelpers.TryParseDuration(Duration, out _);
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}