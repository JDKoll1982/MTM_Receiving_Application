using System.Reflection;
using MTM_Waitlist_Application_2._0.Core;
using MTM_Waitlist_Application_2._0.Core.Commands;
using MTM_Waitlist_Application_2._0.Services;

// Encapsulation Completed. (2024/08/23)

namespace MTM_Waitlist_Application_2._0.ViewModels
{
    public class RequestViewModel : ViewModel
    {
        public RelayCommand NavigateToWaitlist { get; set; }

        private INavigationService _navigation;

        public INavigationService Navigation
        {
            get
            {
                try
                {
                    return _navigation;
                }
                catch (Exception ex)
                {
                    ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                    return null!;
                }
            }
            set
            {
                try
                {
                    _navigation = value;
                    OnPropertyChanged();
                }
                catch (Exception ex)
                {
                    ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                }
            }
        }
    }
}