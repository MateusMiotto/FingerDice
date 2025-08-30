
namespace FingerDice.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
            Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = false });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = false });

        }
    }
}