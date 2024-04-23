using CodedByKay.PowerPatrol.Extensions;
using CodedByKay.PowerPatrol.Interfaces;
using CodedByKay.PowerPatrol.Models;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevExpress.Maui.Charts;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;


namespace CodedByKay.PowerPatrol.ViewModels
{
    public partial class TibberViewModel : BaseViewModel
    {
        private readonly ITibberService _tibberService;
        private readonly IPreferencesService _preferencesService;
        private readonly ApplicationSettings _applicationSettings;
        private readonly IUserPersmissionsService _userPersmissionsService;

        //Today
        [ObservableProperty]
        private bool showTodayChart = false;

        [ObservableProperty]
        private string todayAveragePriceTitle = string.Empty;

        [ObservableProperty]
        private ValueBandPointColorizer todayColorizer;
        //Today

        //Tomorrow
        [ObservableProperty]
        private bool showTomorrowsChart = false;

        [ObservableProperty]
        private string tomorrowAveragePriceTitle = string.Empty;

        [ObservableProperty]
        private ValueBandPointColorizer tomorrowColorizer;
        //Tomorrow

        [ObservableProperty]
        private string tibberAddress;

        [ObservableProperty]
        private string timeTitle;

        [ObservableProperty]
        private DateTime currentTime;

        [ObservableProperty]
        ObservableCollection<EnergyPrice> tibberChartDataToday = [];

        [ObservableProperty]
        ObservableCollection<EnergyPrice> tibberChartDataTomorrow = [];

        public TibberViewModel(
            ITibberService tibberService,
            IPreferencesService preferencesService,
            IOptions<ApplicationSettings> applicationSettings,
            IUserPersmissionsService userPersmissionsService)
        {
            _tibberService = tibberService;
            _preferencesService = preferencesService;
            _userPersmissionsService = userPersmissionsService;
            _applicationSettings = applicationSettings.Value;
        }

        [RelayCommand]
        private async Task RefreshTibberData()
        {
            TibberChartDataToday.Clear();
            TibberChartDataTomorrow.Clear();

            ShowTodayChart = false;
            ShowTomorrowsChart = false;

            await UpdateTime();
            _preferencesService.Clear();
            await GetTibberData();

            await ShowToast("Huuzaa! Grafen är uppdaterad!");
        }

        public async Task GetUserPermissions()
        {
            var cancellactionTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellactionTokenSource.Token;

            var userHasGivenPermissions = await _userPersmissionsService.GetPermissionsFromUser(cancellationToken);
            if (!userHasGivenPermissions)
            {
                await ShowToast("Ooppss! Power patrol behöver internet för att fungera korrent.");
                return;
            }
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

                SetTodayPriceDetails(highestPrice, lowestPrice, todayTotalSum, priceInfo.Today.Count);

                ShowTodayChart = true;
            }
            else
            {
                ShowTodayChart = false;
            }
        }

        private void SetTodayPriceDetails(double highestPrice, double lowestPrice, double todayTotalSum, int tomorrowCount)
        {
            var roundedHighestPriceToday = Math.Ceiling(highestPrice);
            var roundedLowestPriceToday = Math.Floor(lowestPrice);

            // Calculate the average price and round it
            double averagePriceToday = todayTotalSum / tomorrowCount;
            var todayAveragePrice = Math.Round(averagePriceToday, 1);

            var roundedAveragePricetoday = Math.Round(averagePriceToday, 0); // Ensuring no decimals

            // Calculate the segment points based on the rounded average price
            var todaySegmentPointOne = roundedAveragePricetoday - 12;
            var todaySegmentPointTwo = roundedAveragePricetoday + 12;

            // Ensure the segment points do not exceed the bounds of lowest and highest prices
            todaySegmentPointOne = Math.Max(todaySegmentPointOne, roundedLowestPriceToday);
            todaySegmentPointTwo = Math.Min(todaySegmentPointTwo, roundedHighestPriceToday);

            SetTodayAveragePriceTitle(todayAveragePrice);

            TodayColorizer = SetColorizerForChart(roundedLowestPriceToday, todaySegmentPointOne, todaySegmentPointTwo, roundedHighestPriceToday);
        }

        private void SetTodayAveragePriceTitle(double todayAveragePrice)
        {
            TodayAveragePriceTitle = $"Genomsnittspris idag {todayAveragePrice} öre";
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

                SetTomorrowPriceDetails(highestPrice, lowestPrice, tomorrowTotalSum, priceInfo.Tomorrow.Count);

                ShowTomorrowsChart = true;
            }
            else
            {
                ShowTomorrowsChart = false;
            }
        }

        private void SetTomorrowPriceDetails(double highestPrice, double lowestPrice, double tomorrowTotalSum, int tomorrowCount)
        {
            var roundedHighestPriceTomorrow = Math.Ceiling(highestPrice);
            var roundedLowestPriceTomorrow = Math.Floor(lowestPrice);

            // Calculate the average price and round it
            double averagePriceTomorrow = tomorrowTotalSum / tomorrowCount;
            var tomorrowAveragePrice = Math.Round(averagePriceTomorrow, 1);

            var roundedAveragePriceTomorrow = Math.Round(averagePriceTomorrow, 0); // Ensuring no decimals

            // Calculate the segment points based on the rounded average price
            var tomorrowSegmentPointOne = roundedAveragePriceTomorrow - 12;
            var tomorrowSegmentPointTwo = roundedAveragePriceTomorrow + 12;

            // Ensure the segment points do not exceed the bounds of lowest and highest prices
            tomorrowSegmentPointOne = Math.Max(tomorrowSegmentPointOne, roundedLowestPriceTomorrow);
            tomorrowSegmentPointTwo = Math.Min(tomorrowSegmentPointTwo, roundedHighestPriceTomorrow);

            SetTomorrowAveragePriceTitle(tomorrowAveragePrice);

            TomorrowColorizer = SetColorizerForChart(roundedLowestPriceTomorrow, tomorrowSegmentPointOne, tomorrowSegmentPointTwo, roundedHighestPriceTomorrow);
        }

        private void SetTomorrowAveragePriceTitle(double tomorrowAveragePrice)
        {
            TomorrowAveragePriceTitle = $" Genomsnittspris imorgon {tomorrowAveragePrice} öre";
        }

        private ValueBandPointColorizer SetColorizerForChart(double lowest, double segmentPointOne, double segmentPointTwo, double highest)
        {
            var pointColorizer = new ValueBandPointColorizer();

            pointColorizer.ColorStops.Add(new ColorStop() { Color = Color.FromArgb("#6ab04c"), Value1 = lowest, Value2 = segmentPointOne }); ;
            pointColorizer.ColorStops.Add(new ColorStop() { Color = Color.FromArgb("#eba34b"), Value1 = segmentPointOne, Value2 = segmentPointTwo });
            pointColorizer.ColorStops.Add(new ColorStop() { Color = Color.FromArgb("#eb4d4b"), Value1 = segmentPointTwo, Value2 = highest });

            return pointColorizer;
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
