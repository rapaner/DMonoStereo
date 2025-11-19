using System.Globalization;

namespace DMonoStereo.Helpers;

/// <summary>
/// Вспомогательный класс для работы с TimeSpan и форматированием продолжительности.
/// </summary>
public static class TimeSpanHelpers
{
    /// <summary>
    /// Форматирует продолжительность в секундах в текстовое представление.
    /// </summary>
    /// <param name="seconds">Количество секунд.</param>
    /// <returns>Строка в формате "mm:ss" или "h:mm:ss" в зависимости от наличия часов.</returns>
    public static string FormatDuration(int seconds)
    {
        var duration = TimeSpan.FromSeconds(seconds);
        return duration.Hours > 0
            ? duration.ToString(@"h\:mm\:ss")
            : duration.ToString(@"mm\:ss");
    }

    /// <summary>
    /// Парсит строку длительности в секунды.
    /// </summary>
    /// <param name="text">Текст для парсинга. Может быть в формате "mm:ss", "h:mm:ss" или просто число секунд.</param>
    /// <param name="seconds">Результат парсинга - количество секунд.</param>
    /// <returns>true, если парсинг успешен и значение больше 0, иначе false.</returns>
    public static bool TryParseDuration(string? text, out int seconds)
    {
        seconds = 0;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var normalized = text.Trim();
        if (TimeSpan.TryParseExact(normalized, @"m\:ss", CultureInfo.InvariantCulture, out var time) ||
            TimeSpan.TryParseExact(normalized, @"mm\:ss", CultureInfo.InvariantCulture, out time) ||
            TimeSpan.TryParseExact(normalized, @"h\:mm\:ss", CultureInfo.InvariantCulture, out time))
        {
            seconds = (int)Math.Round(time.TotalSeconds);
            return seconds > 0;
        }

        if (int.TryParse(normalized, out var rawSeconds) && rawSeconds > 0)
        {
            seconds = rawSeconds;
            return true;
        }

        return false;
    }
}

