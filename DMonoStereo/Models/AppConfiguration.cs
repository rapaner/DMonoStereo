namespace DMonoStereo.Models;

/// <summary>
/// Конфигурация приложения
/// </summary>
public class AppConfiguration
{
    /// <summary>
    /// Путь к файлу базы данных
    /// </summary>
    public string DatabasePath { get; set; } = string.Empty;

    /// <summary>
    /// Имя файла базы данных
    /// </summary>
    public string DatabaseFileName { get; set; } = "dmonostereo.db";

    /// <summary>
    /// Директория для хранения данных приложения
    /// </summary>
    public string AppDataDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Версия приложения
    /// </summary>
    public string AppVersion { get; set; } = string.Empty;

    /// <summary>
    /// Имя приложения
    /// </summary>
    public string AppName { get; set; } = "DMonoStereo";

    /// <summary>
    /// ClientId для OAuth авторизации Яндекс Диска
    /// </summary>
    public string YandexOAuthClientId { get; set; } = string.Empty;

    /// <summary>
    /// API key для Discogs
    /// </summary>
    public string DiscogsKey { get; set; } = string.Empty;

    /// <summary>
    /// API secret для Discogs
    /// </summary>
    public string DiscogsSecret { get; set; } = string.Empty;
}