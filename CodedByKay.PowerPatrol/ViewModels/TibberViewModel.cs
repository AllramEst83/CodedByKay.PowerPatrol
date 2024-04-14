using CodedByKay.PowerPatrol.EventMessages;
using CodedByKay.PowerPatrol.Extensions;
using CodedByKay.PowerPatrol.Interfaces;
using CodedByKay.PowerPatrol.Models;
using CodedByKay.PowerPatrol.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;

namespace CodedByKay.PowerPatrol.ViewModels
{
    public partial class TibberViewModel : BaseViewModel
    {
        private readonly ITibberService _tibberService;
        private readonly IPreferencesService _preferencesService;
        private readonly ApplicationSettings _applicationSettings;

        [ObservableProperty]
        private double todayAveragePrice;

        [ObservableProperty]
        private double tomorrowAveragePrice;

        [ObservableProperty]
        private string tibberAddress;

        private bool isRegistered = false;

        [ObservableProperty]
        ObservableCollection<EnergyPrice> tibberChartDataToday = [];

        [ObservableProperty]
        ObservableCollection<EnergyPrice> tibberChartDataTomorrow = [];


        private readonly List<string> flatColors =
        [
            "#f3a683", "#f7d794", "#778beb", "#e77f67", "#cf6a87",
            "#f19066", "#f5cd79", "#546de5", "#e15f41", "#c44569",
            "#786fa6", "#f8a5c2", "#63cdda", "#ea8685", "#596275",
            "#574b90", "#f78fb3", "#3dc1d3", "#e66767", "#303952"
        ];

        public TibberViewModel(
            ITibberService tibberService,
            IPreferencesService preferencesService,
            IOptions<ApplicationSettings> applicationSettings)
        {
            _tibberService = tibberService;
            _preferencesService = preferencesService;
            _applicationSettings = applicationSettings.Value;
        }
    
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

        [RelayCommand]
        private async Task RefreshTibberData()
        {
            TibberChartDataToday = [];
            TibberChartDataTomorrow = [];

            _preferencesService.Clear();
            await GetTibberData();
        }

        private CurrentEnergyPrice? GetStoredTibberData()
        {
            var storedData = _preferencesService.Get<CurrentEnergyPrice>(_applicationSettings.TibberHomeDetailsKey);
            if (storedData is null)
            {
                return null;
            }

            return storedData;
        }

        private void CalculateTodaysPricesAndAverage(PriceInfo priceInfo)
        {
            double todayTotalSum = 0;
            if (priceInfo.Today.Count > 0)
            {
                int colorIndexOne = 0;
                foreach (var item in priceInfo.Today)
                {
                    var color = flatColors[colorIndexOne % flatColors.Count];

                    var totalInOre = item.Total * 100;
                    todayTotalSum += totalInOre;

                    var energyPrice = new EnergyPrice(item.StartsAt.ToSwedishTime(), (float)totalInOre, color);

                    TibberChartDataToday.Add(energyPrice);

                    colorIndexOne++;
                }
            }

            double averagePriceToday = todayTotalSum / priceInfo.Today.Count;
            TodayAveragePrice = Math.Round(averagePriceToday, 1);
        }

        private void CalculatetomorrowsPricesAndAverage(PriceInfo priceInfo)
        {
            double tomorrowTotalSum = 0;
            if (priceInfo.Tomorrow.Count > 0)
            {
                int colorIndexTwo = 0;
                foreach (var item in priceInfo.Tomorrow)
                {
                    var color = flatColors[colorIndexTwo % flatColors.Count];
                    var totalInOre = item.Total * 100;
                    tomorrowTotalSum += totalInOre;

                    var energyPrice = new EnergyPrice(item.StartsAt.ToSwedishTime(), (float)totalInOre, color);

                    TibberChartDataTomorrow.Add(energyPrice);

                    colorIndexTwo++;
                }

                double averagePriceTomorrow = tomorrowTotalSum / priceInfo.Tomorrow.Count;
                TomorrowAveragePrice = Math.Round(averagePriceTomorrow, 1);
            }
        }

        private async Task ShowToast(string message)
        {
            await Toast.Make(message, CommunityToolkit.Maui.Core.ToastDuration.Long).Show(CancellationToken.None);
        }
        private async Task GetTibberData()
        {
            CurrentEnergyPrice? storedTibberData;
            storedTibberData = GetStoredTibberData();


            if (storedTibberData is null)
            {
                storedTibberData = await _tibberService.GetEnergyConsumption();
                _preferencesService.Set(_applicationSettings.TibberHomeDetailsKey, storedTibberData);
            }

            TibberAddress = storedTibberData.Address.Address1;
            CurrentSubscription tibberConsumtionData = storedTibberData.CurrentSubscription;

            if (tibberConsumtionData is null)
            {
                await ShowToast("Ooppss! Ett fel inträffade när din data skulle hämtas.");                               
                return;
            }

            CalculateTodaysPricesAndAverage(tibberConsumtionData.PriceInfo);
            CalculatetomorrowsPricesAndAverage(tibberConsumtionData.PriceInfo);

            await ShowToast("Huuzaa! Grafen är uppdaterad!");
        }
    }
}
