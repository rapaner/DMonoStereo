using System.Text.Json.Serialization;

namespace DMonoStereo.Models.Discogs;

/// <summary>
/// Краткая информация о мастере из Discogs.
/// </summary>
public record DiscogsMasterSummary
{
    /// <summary>
    /// Название мастера.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// Ссылка на ресурс в Discogs.
    /// </summary>
    [JsonPropertyName("resource_url")]
    public string? ResourceUrl { get; init; }

    /// <summary>
    /// Ссылка на изображение обложки.
    /// </summary>
    [JsonPropertyName("cover_image")]
    public string? CoverImage { get; init; }

    /// <summary>
    /// Год выпуска мастер-релиза (строкой).
    /// </summary>
    [JsonPropertyName("year")]
    public string? Year { get; init; }

    /// <summary>
    /// Идентификатор результата поиска.
    /// </summary>
    [JsonPropertyName("id")]
    public int? Id { get; init; }

    /// <summary>
    /// Идентификатор мастера.
    /// </summary>
    [JsonPropertyName("master_id")]
    public int? MasterId { get; init; }
}


