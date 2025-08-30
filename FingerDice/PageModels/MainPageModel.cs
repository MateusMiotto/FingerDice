using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FingerDice.PageModels
{
    public partial class MainPageModel : ObservableObject
    {
        private bool _isNavigatedTo;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        bool _isRefreshing;

        [ObservableProperty]
        private string _today = GetGreeting();

        private static string GetGreeting()
        {
            var hour = DateTime.Now.Hour;
            return hour switch
            {
                >= 5 and < 12 => "Bom dia, Caralho",
                >= 12 and < 18 => "Boa tarde, Caralho",
                _ => "Boa noite, Disgraça"
            };
        }

        public MainPageModel(ModalErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }


        [RelayCommand]
        private Task NavigateToWinnerPage()
            => Shell.Current.GoToAsync($"winner");

        [RelayCommand]
        private Task NavigateToGroup()
            => Shell.Current.GoToAsync($"group");
    }
}