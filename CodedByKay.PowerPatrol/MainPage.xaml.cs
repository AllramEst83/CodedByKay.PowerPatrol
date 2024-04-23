using CodedByKay.PowerPatrol.ViewModels;

namespace CodedByKay.PowerPatrol
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageViewModel? viewModel;
        public MainPage()
        {
            InitializeComponent();

            if (Application.Current?.Handler?.MauiContext?.Services is not null)
            {
                viewModel = Application.Current.Handler.MauiContext.Services.GetService<MainPageViewModel>();
                if (viewModel is null)
                {
                    throw new InvalidOperationException("MainPageViewModel service not found.");
                }
            }
            else
            {
                throw new InvalidOperationException("Unable to access services.");
            }

            BindingContext = viewModel;
        }
    }

}
