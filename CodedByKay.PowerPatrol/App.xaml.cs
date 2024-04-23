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

            // Subscribe to unhandled exception events
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            LoadInitialData(serviceProvider);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Log or handle the exception as needed
            Console.WriteLine($"Unhandled exception: {e.ExceptionObject}");
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            // Log or handle the exception as needed
            Console.WriteLine($"Unobserved task exception: {e.Exception}");
            e.SetObserved(); // This prevents the exception from triggering exception escalation policy which, by default, terminates the process.
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

            var preferencesService = serviceProvider.GetService<IPreferencesService>() ?? throw new Exception("IPreferencesService is not registered in the service provider.");

            var tibberService = serviceProvider.GetService<ITibberService>() ?? throw new Exception("ITibberService is not registered in the service provider.");

            var tibberData = await tibberService.GetEnergyConsumption() ?? throw new InvalidOperationException("Tibber data can not be null");

            preferencesService.Set(applicationSettings.TibberHomeDetailsKey, tibberData);

            MainPage = new AppShell();
        }
    }
}
