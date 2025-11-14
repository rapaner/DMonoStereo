using System.Text.Json.Serialization;

namespace DMonoStereo.Models.Discogs;

/// <summary>
/// Ответ Discogs на поиск по базе данных.
/// </summary>
public record DiscogsSearchResponse
{
    /// <summary>
    /// Результаты поиска мастер-релизов.
    /// </summary>
    [JsonPropertyName("results")]
    public List<DiscogsMasterSummary> Results { get; init; } = [];

    /// <summary>
    /// Метаданные пагинации ответа.
    /// </summary>
    [JsonPropertyName("pagination")]
    public DiscogsPagination? Pagination { get; init; }
}

/// <summary>
/// Общая информация о пагинации ответа Discogs.
/// </summary>
public record DiscogsPagination
{
    /// <summary>
    /// Текущая страница.
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; init; }

    /// <summary>
    /// Общее количество доступных страниц.
    /// </summary>
    [JsonPropertyName("pages")]
    public int Pages { get; init; }

    /// <summary>
    /// Количество элементов на странице.
    /// </summary>
    [JsonPropertyName("per_page")]
    public int PerPage { get; init; }

    /// <summary>
    /// Общее количество элементов.
    /// </summary>
    [JsonPropertyName("items")]
    public int Items { get; init; }
}

