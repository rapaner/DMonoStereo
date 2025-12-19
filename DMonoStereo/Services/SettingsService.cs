using DMonoStereo.Models;
using System.Text.Json;

namespace DMonoStereo.Services;

/// <summary>
/// Сервис для работы с настройками приложения
/// </summary>
public class SettingsService
{
    private const string YandexDiskSettingsKey = "DMonoStereo.YandexDiskSettings";
    private const string AppThemePreferenceKey = "DMonoStereo.AppTheme";
    private const string CapitalizationModePreferenceKey = "DMonoStereo.CapitalizationMode";

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

    public AppTheme? GetAppThemeOverride()
    {
        var value = Preferences.Get(AppThemePreferenceKey, string.Empty);
        return value switch
        {
            nameof(AppTheme.Light) => AppTheme.Light,
            nameof(AppTheme.Dark) => AppTheme.Dark,
            _ => null
        };
    }

    public void SetAppThemeOverride(AppTheme? theme)
    {
        if (theme == null || theme == AppTheme.Unspecified)
        {
            Preferences.Remove(AppThemePreferenceKey);
            return;
        }

        Preferences.Set(AppThemePreferenceKey, theme.Value.ToString());
    }

    /// <summary>
    /// Получить режим капитализации букв
    /// </summary>
    public KeyboardFlags GetCapitalizationMode()
    {
        var value = Preferences.Get(CapitalizationModePreferenceKey, nameof(KeyboardFlags.CapitalizeWord));
        return value switch
        {
            nameof(KeyboardFlags.CapitalizeSentence) => KeyboardFlags.CapitalizeSentence,
            _ => KeyboardFlags.CapitalizeWord
        };
    }

    /// <summary>
    /// Сохранить режим капитализации букв
    /// </summary>
    public void SetCapitalizationMode(KeyboardFlags mode)
    {
        var value = mode switch
        {
            KeyboardFlags.CapitalizeSentence => nameof(KeyboardFlags.CapitalizeSentence),
            _ => nameof(KeyboardFlags.CapitalizeWord)
        };
        Preferences.Set(CapitalizationModePreferenceKey, value);
    }
}