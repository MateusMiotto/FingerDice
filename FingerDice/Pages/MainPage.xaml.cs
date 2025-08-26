using FingerDice.Models;
using FingerDice.PageModels;

namespace FingerDice.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}