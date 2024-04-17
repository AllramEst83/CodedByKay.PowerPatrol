using CodedByKay.PowerPatrol.EventMessages;
using CodedByKay.PowerPatrol.Interfaces;
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
            await viewModel.GetUserPermissions();
            await viewModel.GetTibberData();
            await viewModel.UpdateTime();
        }     
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

    }

    private void TodayChecked_CheckedChanged(object sender, EventArgs e)
    {
        TodaysChart.Visible = !TodaysChart.Visible;
    }

    private void TomorrowChecked_CheckedChanged(object sender, EventArgs e)
    {
        TomorrowsChart.Visible = !TomorrowsChart.Visible;
    }

    private void XAxisTimeConstChecked_CheckedChanged(object sender, EventArgs e)
    {
        AxisTimeConst.Visible = !AxisTimeConst.Visible;
    }
}