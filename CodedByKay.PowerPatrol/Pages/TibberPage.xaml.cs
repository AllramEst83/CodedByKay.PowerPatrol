using CodedByKay.PowerPatrol.EventMessages;
using CodedByKay.PowerPatrol.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace CodedByKay.PowerPatrol.Pages;

public partial class TibberPage : ContentPage
{
    private readonly TibberViewModel? viewModel;
    public TibberPage()
    {
        InitializeComponent();

        if (Application.Current?.Handler?.MauiContext?.Services is not null)
        {
            viewModel = Application.Current.Handler.MauiContext.Services.GetService<TibberViewModel>();
            if (viewModel is null)
            {
                throw new InvalidOperationException("TibberViewModel service not found.");
            }
        }
        else
        {
            throw new InvalidOperationException("Unable to access services.");
        }

        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is TibberViewModel viewModel)
        {
            await viewModel.GetTibberData();
            viewModel.UpdateAveragePriceTitles();
            await viewModel.UpdateTime();
            //viewModel.RegisterEvents();
        }

        //MainThread.BeginInvokeOnMainThread(() =>
        //{
        //    WeakReferenceMessenger.Default.Send(new LoadTibberDataEventMessage());
        //});
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (BindingContext is TibberViewModel viewModel)
        {
            viewModel.UnregisterEvents();
        }
    }

    private void TodayChecked_CheckedChanged(object sender, EventArgs e)
    {
        TodaysChart.Visible = !TodaysChart.Visible;
    }

    private void TomorrowChecked_CheckedChanged(object sender, EventArgs e)
    {
        TomorrowsChart.Visible = !TomorrowsChart.Visible;
    }

    private void AverageConstTodayChecked_CheckedChanged(object sender, EventArgs e)
    {
        if (BindingContext is TibberViewModel viewModel)
        {

            viewModel.IsTodayAveragePriceConstantVisible = !viewModel.IsTodayAveragePriceConstantVisible;
        }
    }

    private void AverageConstTomorrowChecked_CheckedChanged(object sender, EventArgs e)
    {
        if (BindingContext is TibberViewModel viewModel)
        {

            viewModel.IsTomorrowAveragePriceConstantVisible = !viewModel.IsTomorrowAveragePriceConstantVisible;
        }
    }


    private void XAxisTimeConstChecked_CheckedChanged(object sender, EventArgs e)
    {
        AxisTimeConst.Visible = !AxisTimeConst.Visible;
    }
}