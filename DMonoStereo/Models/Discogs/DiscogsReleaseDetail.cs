using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DMonoStereo.Models.Discogs;

/// <summary>
/// Подробная информация о релизе Discogs.
/// </summary>
public record DiscogsReleaseDetail
{
    /// <summary>
    /// Идентификатор релиза.
    /// </summary>
    [JsonPropertyName("id")]
    public int? Id { get; init; }

    /// <summary>
    /// Название релиза.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// Страна релиза.
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; init; }

    /// <summary>
    /// Дата выпуска (строкой).
    /// </summary>
    [JsonPropertyName("released")]
    public string? Released { get; init; }

    /// <summary>
    /// Год релиза.
    /// </summary>
    [JsonPropertyName("year")]
    public int? Year { get; init; }

    /// <summary>
    /// Артисты релиза.
    /// </summary>
    [JsonPropertyName("artists")]
    public List<DiscogsReleaseArtist> Artists { get; init; } = new();

    /// <summary>
    /// Треклист релиза.
    /// </summary>
    [JsonPropertyName("tracklist")]
    public List<DiscogsReleaseTrack> Tracklist { get; init; } = new();

    /// <summary>
    /// Изображения релиза.
    /// </summary>
    [JsonPropertyName("images")]
    public List<DiscogsReleaseImage> Images { get; init; } = new();

    /// <summary>
    /// Артист релиза.
    /// </summary>
    public record DiscogsReleaseArtist
    {
        /// <summary>
        /// Имя артиста.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        /// <summary>
        /// Ссылка на миниатюру артиста.
        /// </summary>
        [JsonPropertyName("thumbnail_url")]
        public string? ThumbnailUrl { get; init; }
    }

    /// <summary>
    /// Запись треклиста релиза.
    /// </summary>
    public record DiscogsReleaseTrack
    {
        /// <summary>
        /// Позиция трека.
        /// </summary>
        [JsonPropertyName("position")]
        public string? Position { get; init; }

        /// <summary>
        /// Название трека.
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; init; }

        /// <summary>
        /// Продолжительность трека.
        /// </summary>
        [JsonPropertyName("duration")]
        public string? Duration { get; init; }
    }

    /// <summary>
    /// Изображение релиза.
    /// </summary>
    public record DiscogsReleaseImage
    {
        /// <summary>
        /// Ссылка на ресурс изображения.
        /// </summary>
        [JsonPropertyName("resource_url")]
        public string? ResourceUrl { get; init; }
    }
}


