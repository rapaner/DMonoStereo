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

    /// <summary>
    /// Список доступных фильтров для версий.
    /// </summary>
    [JsonPropertyName("filter_facets")]
    public List<DiscogsFilterFacet> FilterFacets { get; init; } = [];
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
    /// Страна релиза.
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; init; }

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

/// <summary>
/// Фасет фильтра для версий мастер-релиза.
/// </summary>
public record DiscogsFilterFacet
{
    /// <summary>
    /// Идентификатор фасета (например, "format", "country", "released").
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Название фасета.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// Список значений фасета.
    /// </summary>
    [JsonPropertyName("values")]
    public List<DiscogsFilterFacetValue> Values { get; init; } = [];

    /// <summary>
    /// Разрешает ли фасет множественные значения.
    /// </summary>
    [JsonPropertyName("allows_multiple_values")]
    public bool AllowsMultipleValues { get; init; }
}

/// <summary>
/// Значение фасета фильтра.
/// </summary>
public record DiscogsFilterFacetValue
{
    /// <summary>
    /// Отображаемое название значения.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// Значение для использования в фильтрах (может быть URL-encoded).
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; init; }

    /// <summary>
    /// Количество версий, соответствующих этому значению.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; init; }
}