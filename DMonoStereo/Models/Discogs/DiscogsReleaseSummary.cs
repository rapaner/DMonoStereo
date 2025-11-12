using System.Text.Json.Serialization;

namespace DMonoStereo.Models.Discogs;

/// <summary>
/// Краткая информация о релизе из Discogs.
/// </summary>
public class DiscogsReleaseSummary
{
    /// <summary>
    /// Название релиза.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Ссылка на ресурс в Discogs.
    /// </summary>
    [JsonPropertyName("resource_url")]
    public string? ResourceUrl { get; set; }

    /// <summary>
    /// Год релиза.
    /// </summary>
    [JsonPropertyName("year")]
    public int? Year { get; set; }

    /// <summary>
    /// Идентификатор релиза.
    /// </summary>
    [JsonPropertyName("id")]
    public int? Id { get; set; }
}

