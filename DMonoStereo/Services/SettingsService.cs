using System.Text.Json;
using DMonoStereo.Models;

namespace DMonoStereo.Services;

/// <summary>
/// Сервис для работы с настройками приложения
/// </summary>
public class SettingsService
{
    private const string YandexDiskSettingsKey = "DMonoStereo.YandexDiskSettings";

    /// <summary>
    /// Получить сохраненные настройки Яндекс Диска
    /// </summary>
    public YandexDiskSettings GetYandexDiskSettings()
    {
        var json = Preferences.Get(YandexDiskSettingsKey, string.Empty);

        if (string.IsNullOrEmpty(json))
        {
            return new YandexDiskSettings();
        }

        try
        {
            return JsonSerializer.Deserialize<YandexDiskSettings>(json) ?? new YandexDiskSettings();
        }
        catch
        {
            return new YandexDiskSettings();
        }
    }

    /// <summary>
    /// Сохранить настройки Яндекс Диска
    /// </summary>
    public void SaveYandexDiskSettings(YandexDiskSettings settings)
    {
        var json = JsonSerializer.Serialize(settings);
        Preferences.Set(YandexDiskSettingsKey, json);
    }

    /// <summary>
    /// Очистить настройки Яндекс Диска
    /// </summary>
    public void ClearYandexDiskSettings()
    {
        Preferences.Remove(YandexDiskSettingsKey);
    }
}
