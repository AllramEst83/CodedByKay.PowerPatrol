using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
