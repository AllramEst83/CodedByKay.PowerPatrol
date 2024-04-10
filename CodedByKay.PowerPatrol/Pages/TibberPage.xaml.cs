using CodedByKay.PowerPatrol.EventMessages;
using CodedByKay.PowerPatrol.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microcharts;
using SkiaSharp;

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
    protected async override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is TibberViewModel viewModel)
        {
            viewModel.RegisterEvents();
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            WeakReferenceMessenger.Default.Send(new LoadTibberDataEventMessage());
        });
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (BindingContext is TibberViewModel viewModel)
        {
            viewModel.UnregisterEvents();
        }
    }
}