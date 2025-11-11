using DMonoStereo.Models;
using DMonoStereo.Services;

namespace DMonoStereo.Views;

public partial class YandexDiskPage : ContentPage
{
    private readonly YandexDiskService _yandexDiskService;
    private readonly YandexOAuthService _oauthService;
    private readonly SettingsService _settingsService;
    private readonly AppConfiguration _appConfiguration;

    private YandexDisk.Client.Protocol.Resource? _selectedBackup;

    public YandexDiskPage(
        YandexDiskService yandexDiskService,
        YandexOAuthService oauthService,
        SettingsService settingsService,
        AppConfiguration appConfiguration)
    {
        InitializeComponent();

        _yandexDiskService = yandexDiskService;
        _oauthService = oauthService;
        _settingsService = settingsService;
        _appConfiguration = appConfiguration;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        var settings = _settingsService.GetYandexDiskSettings();

        if (!string.IsNullOrEmpty(settings.OAuthToken))
        {
            _yandexDiskService.SetOAuthToken(settings.OAuthToken);
            await UpdateStatusAsync();
            await LoadBackupsAsync();
        }
        else
        {
            StatusLabel.Text = "Не подключено";
            DiskInfoLabel.IsVisible = false;
        }

        if (settings.LastBackupDate.HasValue)
        {
            LastBackupLabel.Text = $"Последняя резервная копия: {settings.LastBackupDate:dd.MM.yyyy HH:mm}";
        }
    }

    private async Task UpdateStatusAsync()
    {
        try
        {
            if (_yandexDiskService.IsAuthorized)
            {
                var info = await _yandexDiskService.GetDiskInfoAsync();
                StatusLabel.Text = "Подключено";
                var totalGb = info.TotalSpace / (1024.0 * 1024.0 * 1024.0);
                var usedGb = info.UsedSpace / (1024.0 * 1024.0 * 1024.0);
                DiskInfoLabel.Text = $"Использовано: {usedGb:F2} ГБ из {totalGb:F2} ГБ";
                DiskInfoLabel.IsVisible = true;
            }
            else
            {
                StatusLabel.Text = "Не подключено";
                DiskInfoLabel.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Ошибка: {ex.Message}";
            DiskInfoLabel.IsVisible = false;
        }
    }

    private async void OnGetTokenClicked(object? sender, EventArgs e)
    {
        try
        {
            await _oauthService.AuthenticateAsync();

            await DisplayAlert(
                "Инструкция",
                "После авторизации скопируйте токен из адресной строки (после #access_token=) и вставьте его в поле.",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async void OnSaveTokenClicked(object? sender, EventArgs e)
    {
        var token = TokenEntry.Text?.Trim();
        if (string.IsNullOrEmpty(token))
        {
            await DisplayAlert("Ошибка", "Введите OAuth токен", "OK");
            return;
        }

        try
        {
            _yandexDiskService.SetOAuthToken(token);
            await _yandexDiskService.GetDiskInfoAsync();

            var settings = _settingsService.GetYandexDiskSettings();
            settings.OAuthToken = token;
            _settingsService.SaveYandexDiskSettings(settings);

            await UpdateStatusAsync();
            await LoadBackupsAsync();

            TokenEntry.Text = string.Empty;
            await DisplayAlert("Успех", "Токен сохранен", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось подключиться к Яндекс Диску: {ex.Message}", "OK");
        }
    }

    private async void OnDisconnectClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Отключение", "Отключить Яндекс Диск?", "Да", "Нет");
        if (!confirm)
        {
            return;
        }

        _settingsService.ClearYandexDiskSettings();
        StatusLabel.Text = "Не подключено";
        DiskInfoLabel.IsVisible = false;
        BackupsCollectionView.ItemsSource = null;
        NoBackupsLabel.IsVisible = false;
        _selectedBackup = null;

        await DisplayAlert("Готово", "Подключение отключено", "OK");
    }

    private async void OnBackupClicked(object? sender, EventArgs e)
    {
        if (!_yandexDiskService.IsAuthorized)
        {
            await DisplayAlert("Ошибка", "Сначала сохраните OAuth токен", "OK");
            return;
        }

        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var success = await _yandexDiskService.BackupDatabaseAsync(_appConfiguration.DatabasePath);
            if (success)
            {
                var settings = _settingsService.GetYandexDiskSettings();
                settings.LastBackupDate = DateTime.Now;
                _settingsService.SaveYandexDiskSettings(settings);

                LastBackupLabel.Text = $"Последняя резервная копия: {DateTime.Now:dd.MM.yyyy HH:mm}";

                await DisplayAlert("Успех", "Резервная копия создана", "OK");
                await LoadBackupsAsync();
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось создать резервную копию", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async void OnRestoreClicked(object? sender, EventArgs e)
    {
        if (!_yandexDiskService.IsAuthorized)
        {
            await DisplayAlert("Ошибка", "Сначала сохраните OAuth токен", "OK");
            return;
        }

        if (_selectedBackup == null)
        {
            await DisplayAlert("Ошибка", "Выберите резервную копию", "OK");
            return;
        }

        var confirm = await DisplayAlert(
            "Восстановление",
            "Восстановить базу данных из выбранной резервной копии? Текущие данные будут заменены.",
            "Восстановить",
            "Отмена");

        if (!confirm)
        {
            return;
        }

        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var success = await _yandexDiskService.RestoreDatabaseAsync(_selectedBackup.Path, _appConfiguration.DatabasePath);
            if (success)
            {
                await DisplayAlert("Успех", "База данных восстановлена. Перезапустите приложение.", "OK");
                Application.Current?.Quit();
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось восстановить базу данных", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async void OnDeleteBackupClicked(object? sender, EventArgs e)
    {
        if (!_yandexDiskService.IsAuthorized)
        {
            await DisplayAlert("Ошибка", "Сначала сохраните OAuth токен", "OK");
            return;
        }

        if (_selectedBackup == null)
        {
            await DisplayAlert("Ошибка", "Выберите резервную копию", "OK");
            return;
        }

        var confirm = await DisplayAlert("Удаление", "Удалить выбранную резервную копию?", "Удалить", "Отмена");
        if (!confirm)
        {
            return;
        }

        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var success = await _yandexDiskService.DeleteFileAsync(_selectedBackup.Path, permanently: true);
            if (success)
            {
                await DisplayAlert("Успех", "Резервная копия удалена", "OK");
                _selectedBackup = null;
                await LoadBackupsAsync();
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось удалить резервную копию", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async void OnRefreshBackupsClicked(object? sender, EventArgs e)
    {
        await LoadBackupsAsync();
    }

    private async Task LoadBackupsAsync()
    {
        if (!_yandexDiskService.IsAuthorized)
        {
            return;
        }

        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var backups = await _yandexDiskService.GetBackupListAsync();
            if (backups.Count > 0)
            {
                BackupsCollectionView.ItemsSource = backups;
                NoBackupsLabel.IsVisible = false;
            }
            else
            {
                BackupsCollectionView.ItemsSource = null;
                NoBackupsLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private void OnBackupSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView collectionView)
        {
            _selectedBackup = collectionView.SelectedItem as YandexDisk.Client.Protocol.Resource;
        }
    }
}