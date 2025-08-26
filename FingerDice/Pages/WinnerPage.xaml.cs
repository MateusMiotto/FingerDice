namespace FingerDice.Pages
{
    public partial class WinnerPage : ContentPage
    {
        public WinnerPage(WinnerPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}