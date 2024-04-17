using CodedByKay.PowerPatrol.Interfaces;
using CodedByKay.PowerPatrol.Models;
using CodedByKay.PowerPatrol.Pages;
using Microsoft.Extensions.Options;

namespace CodedByKay.PowerPatrol
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            MainPage = new InitializationPage();

            LoadInitialData(serviceProvider);
        }

        private async void LoadInitialData(IServiceProvider serviceProvider)
        {
            var appSettingsOption = serviceProvider.GetService<IOptions<ApplicationSettings>>();
            if (appSettingsOption?.Value == null)
            {
                throw new Exception("AppSettings can not be null.");
            }

            // Extract the Value now that we've confirmed it's not null
            var applicationSettings = appSettingsOption.Value;

            var preferencesService = serviceProvider.GetService<IPreferencesService>();
            if (preferencesService == null)
            {
                // Handle the case where the user state service is not registered, e.g., log an error or throw an exception
                throw new Exception("IPreferencesService is not registered in the service provider.");
            }

            var tibberService = serviceProvider.GetService<ITibberService>();
            if (tibberService == null)
            {
                // Handle the case where the user state service is not registered, e.g., log an error or throw an exception
                throw new Exception("ITibberService is not registered in the service provider.");
            }

            var tibberData = await tibberService.GetEnergyConsumption();
            if (tibberData is null)
            {
                throw new InvalidOperationException("Tibber data can not be null");
            }

            preferencesService.Set(applicationSettings.TibberHomeDetailsKey, tibberData);

            MainPage = new AppShell();
        }
    }
}
