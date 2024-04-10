using CodedByKay.PowerPatrol.EventMessages;
using CodedByKay.PowerPatrol.Interfaces;
using CodedByKay.PowerPatrol.Models;
using CodedByKay.PowerPatrol.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microcharts;
using Microsoft.Extensions.Options;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace CodedByKay.PowerPatrol.ViewModels
{
    public partial class TibberViewModel : BaseViewModel
    {
        private readonly ITibberService _tibberService;
        private readonly IPreferencesService _preferencesService;
        private readonly ApplicationSettings _applicationSettings;

        public TibberViewModel(
            ITibberService tibberService,
            IPreferencesService preferencesService,
            IOptions<ApplicationSettings> applicationSettings)
        {
            _tibberService = tibberService;
            _preferencesService = preferencesService;
            _applicationSettings = applicationSettings.Value;

        }

        [ObservableProperty]
        private string tibberAddress;

        [ObservableProperty]
        private PointChart tibberChartDataToday;
        [ObservableProperty]
        private PointChart tibberChartDataTomorrow;
        [ObservableProperty]
        private bool isRefreshing;
        private bool isRegistered = false;



        public void RegisterEvents()
        {
            if (!isRegistered)
            {
                WeakReferenceMessenger.Default.Register<LoadTibberDataEventMessage>(this, async (recipient, message) =>
                {
                    await GetTibberData();
                });
                isRegistered = true;
            }
        }

        public void UnregisterEvents()
        {
            if (isRegistered)
            {
                WeakReferenceMessenger.Default.Unregister<LoadTibberDataEventMessage>(this);
                isRegistered = false;
            }
        }

        private List<string> flatColors = new List<string>
        {
            "#f3a683", "#f7d794", "#778beb", "#e77f67", "#cf6a87",
            "#f19066", "#f5cd79", "#546de5", "#e15f41", "#c44569",
            "#786fa6", "#f8a5c2", "#63cdda", "#ea8685", "#596275",
            "#574b90", "#f78fb3", "#3dc1d3", "#e66767", "#303952"
        };

        [RelayCommand]
        private async Task RefreshConversations()
        {
            IsRefreshing = true;
            _preferencesService.Clear();
            await GetTibberData();
            IsRefreshing = !IsRefreshing;
        }

        private async Task GetTibberData()
        {
            CurrentEnergyPrice storedTibberData;
            storedTibberData = _preferencesService.Get<CurrentEnergyPrice>(_applicationSettings.TibberHomeDetailsKey);
            CurrentSubscription tibberConsumtionData = null;

            if (storedTibberData is null)
            {
                storedTibberData = await _tibberService.GetEnergyConsumption();
                _preferencesService.Set(_applicationSettings.TibberHomeDetailsKey, storedTibberData);
            }

            TibberAddress = storedTibberData.Address.Address1;
            tibberConsumtionData = storedTibberData.CurrentSubscription;

            if (tibberConsumtionData is null)
            {
                await Toast.Make("Ooppss! Ett fel inträffade när din data skulle hämtas.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show(CancellationToken.None);
                return;
            }
            List<ChartEntry> chartEntryListOne = new List<ChartEntry>();
            List<ChartEntry> chartEntryListTwo = new List<ChartEntry>();

            if (tibberConsumtionData.PriceInfo.Today.Count() > 0)
            {
                int colorIndexOne = 0;
                foreach (var item in tibberConsumtionData.PriceInfo.Today)
                {
                    var color = flatColors[colorIndexOne % flatColors.Count];
                    // Convert kronor to öre
                    var totalInOre = Math.Round(item.Total * 100);

                    var chartEntry = new ChartEntry((float)totalInOre) // Ensure this is the correct type conversion
                    {
                        Label = GetSwedishTime(item.StartsAt),
                        // Format as öre, rounding to nearest whole number if necessary
                        ValueLabel = $"{totalInOre} öre",
                        Color = SKColor.Parse(color)
                    };

                    chartEntryListOne.Add(chartEntry);

                    colorIndexOne++;
                }
                var maxTotalToday = (float)tibberConsumtionData.PriceInfo.Today.Max(item => item.Total);

                TibberChartDataToday = new LineChart()
                {
                    Entries = chartEntryListOne,
                    LineMode = LineMode.Spline,
                    LineSize = 20,
                    EnableYFadeOutGradient = true,
                    IsAnimated = true,
                    AnimationDuration = new TimeSpan(0,0,2),
                    LabelTextSize = 20,
                    ValueLabelTextSize = 20,
                    BackgroundColor = SKColor.Parse("#F8EFBA"),
                    LabelColor = SKColor.Parse("#2C3A47"),
                    MaxValue = maxTotalToday,
                    MinValue = 0
                };
            }

            if (tibberConsumtionData.PriceInfo.Tomorrow.Count() > 0)
            {
                int colorIndexTwo = 0;
                foreach (var item in tibberConsumtionData.PriceInfo.Tomorrow)
                {
                    var color = flatColors[colorIndexTwo % flatColors.Count];
                    // Convert kronor to öre
                    var totalInOre = Math.Round(item.Total * 100);

                    var chartEntry = new ChartEntry((float)totalInOre) // Ensure this is the correct type conversion
                    {
                        Label = GetSwedishTime(item.StartsAt),
                        // Format as öre, rounding to nearest whole number if necessary
                        ValueLabel = $"{totalInOre} öre",
                        Color = SKColor.Parse(color)
                    };

                    chartEntryListTwo.Add(chartEntry);

                    colorIndexTwo++;
                }
                var maxTotalTomorow = (float)tibberConsumtionData.PriceInfo.Tomorrow.Max(item => item.Total);

                TibberChartDataTomorrow = new LineChart()
                {
                    Entries = chartEntryListTwo,
                    LineMode = LineMode.Spline,
                    LineSize = 20,
                    EnableYFadeOutGradient = true,
                    IsAnimated = true,
                    AnimationDuration = new TimeSpan(0, 0, 2),
                    LabelTextSize = 20,
                    ValueLabelTextSize = 20,
                    BackgroundColor = SKColor.Parse("#F8EFBA"),
                    LabelColor = SKColor.Parse("#2C3A47"),
                    MaxValue = maxTotalTomorow,
                    MinValue = 0
                };
            }
        }

        private string GetSwedishTime(DateTime utcDateTime)
        {
            // Ensure utcDateTime is treated as UTC
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            // Swedish time zone ID
            string swedishTimeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                                    ? "W. Europe Standard Time"
                                    : "Europe/Stockholm";

            try
            {
                TimeZoneInfo swedishTimeZone = TimeZoneInfo.FindSystemTimeZoneById(swedishTimeZoneId);
                DateTime swedishTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, swedishTimeZone);

                // Format the Swedish time as a string showing only hours and minutes
                return swedishTime.ToString("HH:mm");
            }
            catch (TimeZoneNotFoundException ex)
            {
                Console.WriteLine($"The time zone '{swedishTimeZoneId}' could not be found on this system.");
                // Handle the case where the time zone ID is not found. You could default to UTC or another known ID.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                // Handle unexpected errors
            }

            return "No time";
        }

    }
}
