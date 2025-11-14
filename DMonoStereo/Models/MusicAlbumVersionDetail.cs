namespace DMonoStereo.Models;

/// <summary>
/// Детальная информация о версии альбома (релизе).
/// </summary>
public record MusicAlbumVersionDetail
{
    /// <summary>
    /// Уникальный идентификатор релиза в базе.
    /// </summary>
    public int? Id { get; init; }

    /// <summary>
    /// Название релиза в конкретной версии.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Формат носителя (CD, LP и т.д.).
    /// </summary>
    public string? Format { get; init; }

    /// <summary>
    /// Страна выпуска.
    /// </summary>
    public string? Country { get; init; }

    /// <summary>
    /// Дата выпуска в текстовом виде (например, месяц и день).
    /// </summary>
    public string? Released { get; init; }

    /// <summary>
    /// Год выпуска.
    /// </summary>
    public int? Year { get; init; }

    /// <summary>
    /// Информация об артисте данной версии.
    /// </summary>
    public MusicAlbumVersionArtist? Artist { get; init; }

    /// <summary>
    /// Основное изображение релиза.
    /// </summary>
    public MusicAlbumVersionImage? Image { get; init; }

    /// <summary>
    /// Треклист релиза.
    /// </summary>
    public IReadOnlyList<MusicAlbumVersionTrack> Tracklist { get; init; } = new List<MusicAlbumVersionTrack>();
}

/// <summary>
/// Информация об артисте релиза.
/// </summary>
public record MusicAlbumVersionArtist
{
    /// <summary>
    /// Имя артиста.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Миниатюра изображения артиста.
    /// </summary>
    public byte[]? ThumbnailImageData { get; init; }
}

/// <summary>
/// Изображение релиза.
/// </summary>
public record MusicAlbumVersionImage
{
    /// <summary>
    /// Бинарные данные изображения обложки.
    /// </summary>
    public byte[]? ImageData { get; init; }
}

/// <summary>
/// Трек релиза.
/// </summary>
public record MusicAlbumVersionTrack
{
    /// <summary>
    /// Порядковый номер трека.
    /// </summary>
    public int Position { get; init; }

    /// <summary>
    /// Название трека.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Длительность трека.
    /// </summary>
    public string? Duration { get; init; }
}