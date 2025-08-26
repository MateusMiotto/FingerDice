using CommunityToolkit.Mvvm.Input;
using FingerDice.Models;

namespace FingerDice.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}