using CodedByKay.PowerPatrol.EventMessages;
using CodedByKay.PowerPatrol.Extensions;
using CodedByKay.PowerPatrol.Interfaces;
using CodedByKay.PowerPatrol.Models;
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

        //Today
        [ObservableProperty]
        private double todayAveragePrice = 0;

        [ObservableProperty]
        private bool showTodayChart = false;

        [ObservableProperty]
        private bool isTodayAveragePriceConstantVisible = false;

        [ObservableProperty]
        private bool isTodayAveragePriceToggleVisible = false;

        [ObservableProperty]
        private string todayAveragePriceTitle = string.Empty;

        [ObservableProperty]
        private double lowestPriceToday = 0;

        [ObservableProperty]
        private double highestPriceToday = 0;

        [ObservableProperty]
        double todaySegmentPointOne = 0;

        [ObservableProperty]
        double todaySegmentPointTwo = 0;
        //Today

        //Tomorrow
        [ObservableProperty]
        private double tomorrowAveragePrice = 0;

        [ObservableProperty]
        private bool showTomorrowsChart = false;

        [ObservableProperty]
        private bool isTomorrowAveragePriceConstantVisible = false;

        [ObservableProperty]
        private bool isTomorrowAveragePriceToggleVisible = false;

        [ObservableProperty]
        private string tomorrowAveragePriceTitle = string.Empty;

        [ObservableProperty]
        private double lowestPriceTomorrow = 0;

        [ObservableProperty]
        private double highestPriceTomorrow = 0;

        [ObservableProperty]
        private double tomorrowSegmentPointOne = 0;

        [ObservableProperty]
        private double tomorrowSegmentPointTwo = 0;
        //Tomorrow

        [ObservableProperty]
        private string tibberAddress;

        [ObservableProperty]
        private string timeTitle;

        [ObservableProperty]
        private DateTime currentTime;



        private bool isRegistered = false;

        [ObservableProperty]
        ObservableCollection<EnergyPrice> tibberChartDataToday = [];

        [ObservableProperty]
        ObservableCollection<EnergyPrice> tibberChartDataTomorrow = [];

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
                    await UpdateTime();
                    await GetTibberData();
                    UpdateAveragePriceTitles();
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

            ShowTodayChart = false;
            ShowTomorrowsChart = false;

            TodayAveragePrice = 0;
            TomorrowAveragePrice = 0;

            LowestPriceToday = 0;
            HighestPriceToday = 0;

            LowestPriceTomorrow = 0;
            HighestPriceTomorrow = 0;

            await UpdateTime();
            _preferencesService.Clear();
            await GetTibberData();
            UpdateAveragePriceTitles();

            await ShowToast("Huuzaa! Grafen är uppdaterad!");
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

        private void CalculateTodaysPricesAndAverage(PriceInfo priceInfo, string timeZone)
        {
            double todayTotalSum = 0;
            double highestPrice = double.MinValue;
            double lowestPrice = double.MaxValue;

            if (priceInfo.Today.Count > 0)
            {
                foreach (var item in priceInfo.Today)
                {

                    var totalInOre = item.Total * 100;
                    todayTotalSum += totalInOre;

                    var energyPrice = new EnergyPrice(item.StartsAt.ConvertToTimeZone(timeZone), (float)totalInOre);

                    TibberChartDataToday.Add(energyPrice);

                    if (totalInOre > highestPrice) highestPrice = totalInOre;
                    if (totalInOre < lowestPrice) lowestPrice = totalInOre;
                }

                HighestPriceToday = Math.Ceiling(highestPrice);
                LowestPriceToday = Math.Floor(lowestPrice);

                // Calculate the average price and round it
                double averagePriceToday = todayTotalSum / priceInfo.Today.Count;
                TodayAveragePrice = Math.Round(averagePriceToday, 0); // Ensuring no decimals

                // Calculate the segment points based on the rounded average price
                TodaySegmentPointOne = TodayAveragePrice - 10;
                TodaySegmentPointTwo = TodayAveragePrice + 10;

                // Ensure the segment points do not exceed the bounds of lowest and highest prices
                TodaySegmentPointOne = Math.Max(TodaySegmentPointOne, LowestPriceToday);
                TodaySegmentPointTwo = Math.Min(TodaySegmentPointTwo, HighestPriceToday);

                ShowTodayChart = true;
                IsTodayAveragePriceToggleVisible = true;
                IsTodayAveragePriceConstantVisible = true;

                Console.WriteLine("-----------------------PRICES-----------------------------");
                Console.WriteLine("TODAY");
                Console.WriteLine($"LowestPriceToday: {LowestPriceToday}");
                Console.WriteLine($"SegmentPointOne: {TodaySegmentPointOne}");
                Console.WriteLine($"TodaySegmentPointTwo: {TodaySegmentPointTwo}");
                Console.WriteLine($"HighestPriceToday: {HighestPriceToday}");
                Console.WriteLine("-----------------------PRICES-----------------------------");
            }
            else
            {
                ShowTodayChart = false;
                IsTodayAveragePriceToggleVisible = false;
                IsTodayAveragePriceConstantVisible = false;
            }
        }

        private void CalculateTomorrowsPricesAndAverage(PriceInfo priceInfo, string timeZone)
        {
            double tomorrowTotalSum = 0;
            double highestPrice = double.MinValue;
            double lowestPrice = double.MaxValue;

            if (priceInfo.Tomorrow.Count > 0)
            {
                foreach (var item in priceInfo.Tomorrow)
                {
                    var totalInOre = item.Total * 100;
                    tomorrowTotalSum += totalInOre;

                    var energyPrice = new EnergyPrice(item.StartsAt.ConvertToTimeZone(timeZone), (float)totalInOre);

                    TibberChartDataTomorrow.Add(energyPrice);

                    if (totalInOre > highestPrice) highestPrice = totalInOre;
                    if (totalInOre < lowestPrice) lowestPrice = totalInOre;
                }

                HighestPriceTomorrow = Math.Ceiling(highestPrice);
                LowestPriceTomorrow = Math.Floor(lowestPrice);

                // Calculate the average price and round it
                double averagePriceTomorrow = tomorrowTotalSum / priceInfo.Tomorrow.Count;
                TomorrowAveragePrice = Math.Round(averagePriceTomorrow, 0); // Ensuring no decimals

                // Calculate the segment points based on the rounded average price
                TomorrowSegmentPointOne = TomorrowAveragePrice - 10;
                TomorrowSegmentPointTwo = TomorrowAveragePrice + 10;

                // Ensure the segment points do not exceed the bounds of lowest and highest prices
                TomorrowSegmentPointOne = Math.Max(TomorrowSegmentPointOne, LowestPriceTomorrow);
                TomorrowSegmentPointTwo = Math.Min(TomorrowSegmentPointTwo, HighestPriceTomorrow);

                ShowTomorrowsChart = true;
                IsTomorrowAveragePriceToggleVisible = true;
                IsTomorrowAveragePriceConstantVisible = true;

                Console.WriteLine("-----------------------PRICES-----------------------------");
                Console.WriteLine("TOMORROW");
                Console.WriteLine($"LowestPriceToday: {LowestPriceTomorrow}");
                Console.WriteLine($"TomorrowSegmentPointOne: {TomorrowSegmentPointOne}");
                Console.WriteLine($"TomorrowSegmentPointTwo: {TomorrowSegmentPointTwo}");
                Console.WriteLine($"HighestPriceToday: {HighestPriceTomorrow}");
                Console.WriteLine("-----------------------PRICES-----------------------------");              
            }
            else
            {
                ShowTomorrowsChart = false;
                IsTomorrowAveragePriceToggleVisible = false;
                IsTomorrowAveragePriceConstantVisible = false;
            }
        }

        private static async Task ShowToast(string message)
        {
            await Toast.Make(message, CommunityToolkit.Maui.Core.ToastDuration.Long).Show(CancellationToken.None);
        }

        public async Task UpdateTime()
        {
            var (tibberData, timeZoneId) = await _tibberService.GetCurrentConsumtion();
            if (tibberData is null)
            {
                throw new InvalidOperationException("Tibber data can not be null");
            }

            var currentConsumtion = tibberData.PriceInfo.Current.Total * 100;
            var roundedConsumtion = Math.Round(currentConsumtion, 1);

            CurrentTime = DateTime.Now.ConvertToTimeZone(timeZoneId);
            TimeTitle = $"{CurrentTime:HH:mm} - {roundedConsumtion} öre";
        }

        [RelayCommand]
        private async Task UpdateCurrentTimeAsync()
        {
            await UpdateTime();
            await ShowToast("Huuzaa! Tiden är uppdaterad.");
        }

        public void UpdateAveragePriceTitles()
        {
            TodayAveragePriceTitle = $"{TodayAveragePrice} öre - Genomsnittspris idag";
            TomorrowAveragePriceTitle = $"{TomorrowAveragePrice} öre - Genomsnittspris imorgon";
        }

        public async Task GetTibberData()
        {
            CurrentEnergyPrice? storedTibberData;
            storedTibberData = GetStoredTibberData();


            if (storedTibberData is null)
            {
                storedTibberData = await _tibberService.GetEnergyConsumption();
                if (storedTibberData is null)
                {
                    await ShowToast("Ooppss! Ett fel inträffade när din data skulle hämtas.");
                }

                _preferencesService.Set(_applicationSettings.TibberHomeDetailsKey, storedTibberData);
            }

            TibberAddress = storedTibberData.Address.Address1;
            CurrentSubscription tibberConsumtionData = storedTibberData.CurrentSubscription;   

            if (tibberConsumtionData is null)
            {
                await ShowToast("Ooppss! Ett fel inträffade när din data skulle hämtas.");
                return;
            }

            string timeZone = storedTibberData.TimeZone;

            CalculateTodaysPricesAndAverage(tibberConsumtionData.PriceInfo, timeZone);
            CalculateTomorrowsPricesAndAverage(tibberConsumtionData.PriceInfo, timeZone);
        }
    }
}
