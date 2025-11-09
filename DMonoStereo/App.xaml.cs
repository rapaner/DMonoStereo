using System.Linq;
using DMonoStereo.Resources.Styles;
using Microsoft.Extensions.DependencyInjection;

namespace DMonoStereo
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            InitializeComponent();

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

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = _serviceProvider.GetRequiredService<AppShell>();
            return new Window(shell);
        }
    }
}