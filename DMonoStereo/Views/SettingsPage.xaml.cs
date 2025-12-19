using DMonoStereo.Models;
using DMonoStereo.Services;
using Microsoft.Maui.Controls;

namespace DMonoStereo.Views;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsService _settingsService;
    private readonly MusicService _musicService;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppConfiguration _appConfiguration;

    public SettingsPage(
        SettingsService settingsService,
        MusicService musicService,
        IServiceProvider serviceProvider,
        AppConfiguration appConfiguration)
    {
        InitializeComponent();

        _settingsService = settingsService;
        _musicService = musicService;
        _serviceProvider = serviceProvider;
        _appConfiguration = appConfiguration;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadInfo();
    }

    private void LoadInfo()
    {
        AppNameLabel.Text = _appConfiguration.AppName;
        VersionLabel.Text = $"Версия {_appConfiguration.AppVersion}";

        LoadThemePreference();
        LoadCapitalizationPreference();
        LoadBackupInfo();
    }

    private void LoadThemePreference()
    {
        var themeOverride = _settingsService.GetAppThemeOverride();
        ThemePicker.SelectedIndex = themeOverride switch
        {
            AppTheme.Light => 1,
            AppTheme.Dark => 2,
            _ => 0
        };

        ThemeDescriptionLabel.Text = themeOverride switch
        {
            AppTheme.Light => "Тема зафиксирована в светлом режиме",
            AppTheme.Dark => "Тема зафиксирована в темном режиме",
            _ => "Тема следует настройкам системы"
        };
    }

    private void LoadCapitalizationPreference()
    {
        var mode = _settingsService.GetCapitalizationMode();
        CapitalizationPicker.SelectedIndex = mode == KeyboardFlags.CapitalizeSentence ? 1 : 0;

        CapitalizationDescriptionLabel.Text = mode switch
        {
            KeyboardFlags.CapitalizeSentence => "Каждое новое предложение начинается с заглавной буквы",
            _ => "Каждое новое слово начинается с заглавной буквы"
        };
    }

    private void OnCapitalizationChanged(object? sender, EventArgs e)
    {
        var mode = CapitalizationPicker.SelectedIndex switch
        {
            1 => KeyboardFlags.CapitalizeSentence,
            _ => KeyboardFlags.CapitalizeWord
        };

        _settingsService.SetCapitalizationMode(mode);

        CapitalizationDescriptionLabel.Text = mode switch
        {
            KeyboardFlags.CapitalizeSentence => "Каждое новое предложение начинается с заглавной буквы",
            _ => "Каждое новое слово начинается с заглавной буквы"
        };

        // Отправить сообщение об изменении настройки для обновления всех Entry
        MessagingCenter.Send(this, "CapitalizationModeChanged", mode);
    }

    private void LoadBackupInfo()
    {
        var settings = _settingsService.GetYandexDiskSettings();
        if (settings.LastBackupDate.HasValue)
        {
            LastBackupLabel.Text = $"Последняя резервная копия: {settings.LastBackupDate:dd.MM.yyyy HH:mm}";
        }
        else
        {
            LastBackupLabel.Text = "Резервные копии еще не создавались";
        }
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        AppTheme? theme = ThemePicker.SelectedIndex switch
        {
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => null
        };

        _settingsService.SetAppThemeOverride(theme);
        Application.Current!.UserAppTheme = theme ?? AppTheme.Unspecified;

        ThemeDescriptionLabel.Text = theme switch
        {
            AppTheme.Light => "Тема зафиксирована в светлом режиме",
            AppTheme.Dark => "Тема зафиксирована в темном режиме",
            _ => "Тема следует настройкам системы"
        };
    }

    private async void OnOpenYandexDiskClicked(object? sender, EventArgs e)
    {
        var page = _serviceProvider.GetRequiredService<YandexDiskPage>();
        await Navigation.PushAsync(page);
    }

    private async void OnClearDataClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlertAsync(
            "Очистка данных",
            "Удалить всех исполнителей, альбомы и треки? Это действие нельзя отменить.",
            "Удалить",
            "Отмена");

        if (!confirm)
        {
            return;
        }

        try
        {
            await _musicService.ClearLibraryAsync();
            await DisplayAlertAsync("Готово", "Все данные удалены.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", $"Не удалось очистить данные: {ex.Message}", "OK");
        }
    }
}