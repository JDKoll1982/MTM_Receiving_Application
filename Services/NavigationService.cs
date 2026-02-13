using MTM_Waitlist_Application_2._0.Core;
using System.Reflection;

// Encapsulation completed. (2024/07/30)

namespace MTM_Waitlist_Application_2._0.Services
{
    public interface INavigationService
    {
        ViewModel CurrentView { get; }
        void NavigateTo<T>() where T : ViewModel;
    }

    public class NavigationService : ViewModel, INavigationService
    {
        private ViewModel _currentView = null!;
        public Func<Type, ViewModel> ViewModelFactory { get; } = null!;

        public ViewModel CurrentView
        {
            get
            {
                try
                {
                    return _currentView;
                }
                catch (Exception ex)
                {
                    ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                    return null!;
                }
            }
            private set
            {
                try
                {
                    _currentView = value;
                    OnPropertyChanged();
                }
                catch (Exception ex)
                {
                    ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                }
            }
        }

        public NavigationService(Func<Type, ViewModel> viewModelFactory)
        {
            try
            {
                ViewModelFactory = viewModelFactory;
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public void NavigateTo<TViewModel>() where TViewModel : ViewModel
        {
            try
            {
                var viewModel = ViewModelFactory.Invoke(typeof(TViewModel));
                CurrentView = viewModel;
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }
    }
}

