using System.Text.Json.Serialization;

namespace DMonoStereo.Models.Discogs;

/// <summary>
/// Подробная информация о мастер-релизе Discogs.
/// </summary>
public record DiscogsMasterDetail
{
    /// <summary>
    /// Название мастер-релиза.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// Год выпуска.
    /// </summary>
    [JsonPropertyName("year")]
    public int? Year { get; init; }

    /// <summary>
    /// Список изображений.
    /// </summary>
    [JsonPropertyName("images")]
    public List<DiscogsMasterImage> Images { get; init; } = new();

    /// <summary>
    /// Треклист.
    /// </summary>
    [JsonPropertyName("tracklist")]
    public List<DiscogsMasterTrack> Tracklist { get; init; } = new();

    /// <summary>
    /// Артисты.
    /// </summary>
    [JsonPropertyName("artists")]
    public List<DiscogsMasterArtist> Artists { get; init; } = new();

    /// <summary>
    /// Информация об изображении мастер-релиза.
    /// </summary>
    public record DiscogsMasterImage
    {
        /// <summary>
        /// Ссылка на изображение.
        /// </summary>
        [JsonPropertyName("uri")]
        public string? Uri { get; init; }
    }

    /// <summary>
    /// Информация о треке мастер-релиза.
    /// </summary>
    public record DiscogsMasterTrack
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

        /// <summary>
        /// Тип элемента треклиста.
        /// </summary>
        [JsonPropertyName("type_")]
        public string? Type { get; init; }
    }

    /// <summary>
    /// Информация об артисте мастер-релиза.
    /// </summary>
    public record DiscogsMasterArtist
    {
        /// <summary>
        /// Имя артиста.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        /// <summary>
        /// Идентификатор артиста.
        /// </summary>
        [JsonPropertyName("id")]
        public int? Id { get; init; }

        /// <summary>
        /// Ссылка на ресурс артиста.
        /// </summary>
        [JsonPropertyName("resource_url")]
        public string? ResourceUrl { get; init; }
    }
}