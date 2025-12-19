using CommunityToolkit.Mvvm.Messaging;
using DMonoStereo.Messages;
using DMonoStereo.Services;
using Microsoft.Maui.Controls;

namespace DMonoStereo.Behaviors;

/// <summary>
/// Attached Property для автоматического применения настройки капитализации к Entry
/// </summary>
public static class KeyboardCapitalizationBehavior
{
    private static SettingsService? _settingsService;
    private static KeyboardFlags _currentMode = KeyboardFlags.CapitalizeWord;

    public static readonly BindableProperty IsEnabledProperty = BindableProperty.CreateAttached(
        "IsEnabled",
        typeof(bool),
        typeof(KeyboardCapitalizationBehavior),
        false,
        propertyChanged: OnIsEnabledChanged);

    public static bool GetIsEnabled(BindableObject view)
    {
        return (bool)view.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(BindableObject view, bool value)
    {
        view.SetValue(IsEnabledProperty, value);
    }

    /// <summary>
    /// Инициализация Behavior с сервисом настроек
    /// </summary>
    public static void Initialize(SettingsService settingsService)
    {
        _settingsService = settingsService;
        _currentMode = settingsService.GetCapitalizationMode();

        // Подписаться на изменения настройки
        WeakReferenceMessenger.Default.Register<CapitalizationModeChangedMessage>(
            null,
            (recipient, message) =>
            {
                _currentMode = message.Mode;
                UpdateAllEntries();
            });
    }

    private static void OnIsEnabledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Entry entry && (bool)newValue)
        {
            ApplyCapitalization(entry);

            // Подписаться на изменения настройки для этого конкретного Entry
            WeakReferenceMessenger.Default.Register<CapitalizationModeChangedMessage>(
                entry,
                (recipient, message) =>
                {
                    _currentMode = message.Mode;
                    ApplyCapitalization(entry);
                });
        }
        else if (bindable is Entry entryToUnregister && !(bool)newValue)
        {
            // Отписаться при отключении
            WeakReferenceMessenger.Default.Unregister<CapitalizationModeChangedMessage>(entryToUnregister);
        }
    }

    private static void ApplyCapitalization(Entry entry)
    {
        if (_settingsService == null)
        {
            return;
        }

        // Применить настройку только если у Entry не указан явно Keyboard (Numeric, Email и т.д.)
        // Проверяем, что Keyboard не был явно установлен в XAML
        var hasExplicitKeyboard = entry.Keyboard != null && 
                                   entry.Keyboard != Keyboard.Default &&
                                   entry.Keyboard != Keyboard.Create(KeyboardFlags.None);

        if (!hasExplicitKeyboard)
        {
            entry.Keyboard = Keyboard.Create(_currentMode);
        }
    }

    private static void UpdateAllEntries()
    {
        // Найти все Entry в текущем приложении и обновить их
        var windows = Application.Current?.Windows ?? [];
        foreach (var window in windows)
        {
            UpdateEntriesInPage(window.Page);
        }
    }

    private static void UpdateEntriesInPage(Page? page)
    {
        if (page == null)
        {
            return;
        }

        // Найти все Entry на странице
        var entries = FindEntries(page);
        foreach (var entry in entries)
        {
            if (GetIsEnabled(entry))
            {
                ApplyCapitalization(entry);
            }
        }
    }

    private static List<Entry> FindEntries(VisualElement element)
    {
        var entries = new List<Entry>();

        if (element is Entry entry)
        {
            entries.Add(entry);
        }

        if (element is Layout layout)
        {
            foreach (var child in layout.Children)
            {
                if (child is VisualElement visualChild)
                {
                    entries.AddRange(FindEntries(visualChild));
                }
            }
        }

        if (element is ContentView contentView && contentView.Content is VisualElement content)
        {
            entries.AddRange(FindEntries(content));
        }

        if (element is ScrollView scrollView && scrollView.Content is VisualElement scrollContent)
        {
            entries.AddRange(FindEntries(scrollContent));
        }

        return entries;
    }
}
