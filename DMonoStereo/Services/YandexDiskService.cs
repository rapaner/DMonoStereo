using YandexDisk.Client;
using YandexDisk.Client.Http;
using YandexDisk.Client.Protocol;

namespace DMonoStereo.Services;

/// <summary>
/// Сервис для работы с Яндекс Диском
/// </summary>
public class YandexDiskService
{
    private const string BackupFolder = "/DMonoStereo_Backups";

    private IDiskApi? _diskApi;
    private string? _oauthToken;

    /// <summary>
    /// Проверяет, авторизован ли пользователь
    /// </summary>
    public bool IsAuthorized => !string.IsNullOrEmpty(_oauthToken) && _diskApi != null;

    /// <summary>
    /// Установить OAuth токен для авторизации
    /// </summary>
    public void SetOAuthToken(string oauthToken)
    {
        _oauthToken = oauthToken;
        _diskApi = new DiskHttpApi(oauthToken);
    }

    /// <summary>
    /// Получить информацию о диске
    /// </summary>
    public async Task<Disk> GetDiskInfoAsync()
    {
        EnsureAuthorized();
        return await _diskApi!.MetaInfo.GetDiskInfoAsync();
    }

    /// <summary>
    /// Загрузить файл базы данных на Яндекс Диск
    /// </summary>
    public async Task<bool> UploadFileAsync(string localFilePath, string remotePath, bool overwrite = true)
    {
        EnsureAuthorized();

        if (!File.Exists(localFilePath))
        {
            throw new FileNotFoundException($"Файл не найден: {localFilePath}");
        }

        var directory = Path.GetDirectoryName(remotePath)
            ?.Replace("\\", "/");
        if (!string.IsNullOrEmpty(directory) && directory != "/")
        {
            await CreateDirectoryAsync(directory);
        }

        var link = await _diskApi!.Files.GetUploadLinkAsync(remotePath, overwrite);
        await using var fileStream = File.OpenRead(localFilePath);
        await _diskApi.Files.UploadAsync(link, fileStream);

        return true;
    }

    /// <summary>
    /// Скачать файл с Яндекс Диска
    /// </summary>
    public async Task<bool> DownloadFileAsync(string remotePath, string localFilePath)
    {
        EnsureAuthorized();

        var link = await _diskApi!.Files.GetDownloadLinkAsync(remotePath);
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(link.Href);
        response.EnsureSuccessStatusCode();

        var directory = Path.GetDirectoryName(localFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fileStream = File.Create(localFilePath);
        await response.Content.CopyToAsync(fileStream);

        return true;
    }

    public async Task<YandexDisk.Client.Protocol.Resource> GetFilesAsync(string remotePath)
    {
        EnsureAuthorized();
        return await _diskApi!.MetaInfo.GetInfoAsync(new ResourceRequest { Path = remotePath });
    }

    public async Task<bool> CreateDirectoryAsync(string remotePath)
    {
        EnsureAuthorized();

        try
        {
            var resource = await _diskApi!.MetaInfo.GetInfoAsync(new ResourceRequest { Path = remotePath });
            if (resource != null && resource.Type == ResourceType.Dir)
            {
                return true;
            }
        }
        catch
        {
            // ignore and create directory
        }

        await _diskApi!.Commands.CreateDictionaryAsync(remotePath);
        return true;
    }

    public async Task<bool> DeleteFileAsync(string remotePath, bool permanently = false)
    {
        EnsureAuthorized();
        await _diskApi!.Commands.DeleteAsync(new DeleteFileRequest { Path = remotePath, Permanently = permanently });
        return true;
    }

    /// <summary>
    /// Создать резервную копию базы данных
    /// </summary>
    public async Task<bool> BackupDatabaseAsync(string dbPath)
    {
        EnsureAuthorized();

        if (!File.Exists(dbPath))
        {
            throw new FileNotFoundException($"Файл базы данных не найден: {dbPath}");
        }

        await CreateDirectoryAsync(BackupFolder);

        var remoteFileName = $"{BackupFolder}/dmonostereo_{DateTime.Now:yyyyMMdd_HHmmss}.dbb";
        return await UploadFileAsync(dbPath, remoteFileName, overwrite: false);
    }

    /// <summary>
    /// Восстановить базу данных из резервной копии
    /// </summary>
    public async Task<bool> RestoreDatabaseAsync(string remotePath, string localDbPath)
    {
        EnsureAuthorized();

        if (File.Exists(localDbPath))
        {
            var backupPath = $"{localDbPath}.backup_{DateTime.Now:yyyyMMdd_HHmmss}";
            File.Copy(localDbPath, backupPath, overwrite: true);
        }

        return await DownloadFileAsync(remotePath, localDbPath);
    }

    /// <summary>
    /// Получить список резервных копий
    /// </summary>
    public async Task<List<YandexDisk.Client.Protocol.Resource>> GetBackupListAsync()
    {
        EnsureAuthorized();

        try
        {
            var resource = await _diskApi!.MetaInfo.GetInfoAsync(new ResourceRequest { Path = BackupFolder });
            if (resource?.Embedded?.Items != null)
            {
                return resource.Embedded.Items
                    .Where(item => item.Type == ResourceType.File && (item.Name.EndsWith(".db") || item.Name.EndsWith(".dbb")))
                    .OrderByDescending(item => item.Created)
                    .ToList();
            }
        }
        catch
        {
            // ignore
        }

        return new List<YandexDisk.Client.Protocol.Resource>();
    }

    private void EnsureAuthorized()
    {
        if (!IsAuthorized)
        {
            throw new InvalidOperationException("Необходима авторизация в Яндекс Диске. Сохраните OAuth токен.");
        }
    }
}