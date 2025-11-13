using System;
using System.Collections.Generic;

namespace DMonoStereo.Models;

/// <summary>
/// Ответ расширенного поиска альбомов с поддержкой пагинации.
/// </summary>
public record MusicAlbumSearchResponse
{
    /// <summary>
    /// Список результатов поиска альбомов.
    /// </summary>
    public IReadOnlyList<MusicAlbumSearchResult> Results { get; init; } = Array.Empty<MusicAlbumSearchResult>();

    /// <summary>
    /// Общее количество страниц, доступных в источнике данных.
    /// </summary>
    public int TotalPages { get; init; }
}

