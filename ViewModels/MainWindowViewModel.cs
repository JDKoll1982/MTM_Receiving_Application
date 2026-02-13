using System.Reflection;
using MTM_Waitlist_Application_2._0.Core;
using MTM_Waitlist_Application_2._0.Core.Commands;
using MTM_Waitlist_Application_2._0.Services;
using MTM_Waitlist_Application_2._0.Stores;
using MTM_Waitlist_Application_2._0.WinForms.New_Request.Windows;

// Encapsulation Complete. (2024/07/30)

namespace MTM_Waitlist_Application_2._0.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
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

        public RelayCommand NavigateToWaitlist { get; set; }

        public RelayCommand NavigateToRequest { get; set; }

        public RelayCommand NavigateToMessenger { get; set; }

        public RelayCommand NavigateToHistory { get; set; }

        public RelayCommand NavigateToAnalytics { get; set; }

        public MainWindowViewModel(INavigationService navService)
        {
            try
            {
                Navigation = navService;
                NavigateToWaitlist = new RelayCommand(_ => Navigation.NavigateTo<WaitlistViewModel>(), _ => true);
                NavigateToRequest = new RelayCommand(_ =>
                {
                    NewRequest_Window_RequestType newRequestWindow = new NewRequest_Window_RequestType();
                    newRequestWindow.ShowDialog();
                }, _ => true);
                NavigateToMessenger = new RelayCommand(_ => Navigation.NavigateTo<MessengerViewModel>(), _ => true);
                NavigateToHistory = new RelayCommand(_ => Navigation.NavigateTo<HistoryViewModel>(), _ => true);
                NavigateToAnalytics = new RelayCommand(_ => Navigation.NavigateTo<AnalyticsViewModel>(), _ => true);
                Navigation.NavigateTo<WaitlistViewModel>(); // Default view
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }
    }
}
