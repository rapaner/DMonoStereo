using System.Text;
using DMonoStereo.Models;

namespace DMonoStereo.Services;

/// <summary>
/// Сервис для OAuth авторизации в Яндекс
/// </summary>
public class YandexOAuthService
{
    private readonly string _clientId;

    public YandexOAuthService(AppConfiguration appConfiguration)
    {
        _clientId = appConfiguration.YandexOAuthClientId;
    }

    /// <summary>
    /// Запустить процесс OAuth авторизации для ручного копирования токена
    /// </summary>
    public async Task AuthenticateAsync()
    {
        if (string.IsNullOrWhiteSpace(_clientId))
        {
            throw new InvalidOperationException("Client ID для Яндекс OAuth не настроен. Укажите значение в конфигурации.");
        }

        var authUrl = BuildAuthUrl();
        await Browser.OpenAsync(authUrl, BrowserLaunchMode.SystemPreferred);
    }

    private string BuildAuthUrl()
    {
        var sb = new StringBuilder();
        sb.Append("https://oauth.yandex.ru/authorize?");
        sb.Append("response_type=token");
        sb.Append($"&client_id={_clientId}");
        sb.Append("&display=popup");

        return sb.ToString();
    }
}
