using System.Text.Json.Serialization;

namespace DMonoStereo.Models.Discogs;

/// <summary>
/// Ответ Discogs на поиск по базе данных.
/// </summary>
public class DiscogsSearchResponse
{
    /// <summary>
    /// Результаты поиска.
    /// </summary>
    [JsonPropertyName("results")]
    public List<DiscogsReleaseSummary> Results { get; set; } = new();
}

