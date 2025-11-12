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
}

