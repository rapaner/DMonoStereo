using DMonoStereo.Resources.Styles;
using DMonoStereo.Services;

namespace DMonoStereo
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SettingsService _settingsService;

        public App(IServiceProvider serviceProvider, SettingsService settingsService)
        {
            _serviceProvider = serviceProvider;
            _settingsService = settingsService;

            InitializeComponent();

            ApplyThemeOverride();
            LoadTheme();
            RequestedThemeChanged += OnRequestedThemeChanged;
        }

        private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
        {
            LoadTheme();
        }

        private void LoadTheme()
        {
            var colorDictionaries = Resources.MergedDictionaries
                .Where(d => d is ColorsLight or ColorsDark)
                .ToList();

            foreach (var dictionary in colorDictionaries)
            {
                Resources.MergedDictionaries.Remove(dictionary);
            }

            ResourceDictionary themeDictionary = RequestedTheme == AppTheme.Dark
                ? new ColorsDark()
                : new ColorsLight();

            Resources.MergedDictionaries.Add(themeDictionary);
        }

        private void ApplyThemeOverride()
        {
            var themeOverride = _settingsService.GetAppThemeOverride();
            UserAppTheme = themeOverride ?? AppTheme.Unspecified;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = _serviceProvider.GetRequiredService<AppShell>();
            return new Window(shell);
        }
    }
}