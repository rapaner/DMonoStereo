using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DMonoStereo.Models.Discogs;

/// <summary>
/// Ответ Discogs со списком версий мастер-релиза.
/// </summary>
public record DiscogsMasterVersionsResponse
{
    /// <summary>
    /// Список версий мастера.
    /// </summary>
    [JsonPropertyName("versions")]
    public List<DiscogsMasterVersionSummary> Versions { get; init; } = [];

    /// <summary>
    /// Информация о пагинации.
    /// </summary>
    [JsonPropertyName("pagination")]
    public DiscogsPagination? Pagination { get; init; }
}

/// <summary>
/// Краткая информация о версии мастер-релиза.
/// </summary>
public record DiscogsMasterVersionSummary
{
    /// <summary>
    /// Идентификатор версии.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>
    /// Название версии.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// Формат релиза.
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; init; }

    /// <summary>
    /// Ссылка на ресурс версии.
    /// </summary>
    [JsonPropertyName("resource_url")]
    public string? ResourceUrl { get; init; }

    /// <summary>
    /// Ссылка на миниатюру.
    /// </summary>
    [JsonPropertyName("thumb")]
    public string? Thumb { get; init; }
}


