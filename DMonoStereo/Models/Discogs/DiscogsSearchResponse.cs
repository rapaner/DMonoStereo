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
    public DiscogsSearchPagination? Pagination { get; init; }
}

/// <summary>
/// Информация о пагинации поиска в Discogs.
/// </summary>
public record DiscogsSearchPagination
{
    /// <summary>
    /// Общее количество доступных страниц.
    /// </summary>
    [JsonPropertyName("pages")]
    public int Pages { get; init; }
}

