using CodedByKay.PowerPatrol.Interfaces;
using CodedByKay.PowerPatrol.Models;
using CodedByKay.PowerPatrol.Pages;
using CodedByKay.PowerPatrol.Services;
using CodedByKay.PowerPatrol.ViewModels;
using CommunityToolkit.Maui;
using Microcharts.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using SkiaSharp.Views.Maui.Controls.Hosting;
using System.Diagnostics;

namespace CodedByKay.PowerPatrol
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseSkiaSharp()
                .UseMicrocharts()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            //Appsettings configuration
            var assembly = typeof(MauiProgram).Assembly;
            string configFileName = string.Empty;

#if DEBUG
            configFileName = "CodedByKay.PowerPatrol.appsettings.Local.json";
#else
            configFileName = "CodedByKay.PowerPatrol.appsettings.json";
#endif

            LoadConfiguration(builder);

            builder.Services.AddOptions<ApplicationSettings>()
                    .Bind(builder.Configuration.GetSection("ApplicationSettings"));

            builder.Services.AddHttpClient("PowerPatrolClient", client =>
            {
                var appSettings = builder.Configuration.GetSection("ApplicationSettings").Get<ApplicationSettings>();
                if (appSettings == null)
                {
                    throw new NullReferenceException("AppSettings can not be null.");
                }

                client.BaseAddress = new Uri(appSettings.TibberApiUrl);
            });

            builder.Services
                     //Services
                     .AddSingleton<ITibberService, TibberService>()
                     .AddSingleton<IPreferencesService, PreferencesService>()

            //Pages
             .AddSingleton<MainPage>()
             .AddSingleton<TibberPage>()
             .AddSingleton<EaseePage>()

            //ViewModels
            .AddSingleton<MainPageViewModel>()
            .AddSingleton<TibberViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }


        private static void LoadConfiguration(MauiAppBuilder builder)
        {
            var configFileName = GetConfigurationFileName();
            var assembly = typeof(MauiProgram).Assembly;

            using (var stream = assembly.GetManifestResourceStream(configFileName))
            {
                if (stream == null) throw new FileNotFoundException("Could not find configuration file.", configFileName);

                var config = new ConfigurationBuilder()
                    .AddJsonStream(stream)
                    .Build();

                builder.Configuration.AddConfiguration(config);
            }
        }

        private static string GetConfigurationFileName()
        {
            return typeof(MauiProgram).Assembly.GetName().Name +
                   (Debugger.IsAttached ? ".appsettings.Local.json" : ".appsettings.json");
        }
    }
}
