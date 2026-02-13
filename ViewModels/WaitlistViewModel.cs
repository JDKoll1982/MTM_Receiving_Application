using MTM_Waitlist_Application_2._0.Core;
using MTM_Waitlist_Application_2._0.Stores;
using System.Reflection;

// Encapsulation completed. (2024/07/30)

namespace MTM_Waitlist_Application_2._0.ViewModels
{
    public class WaitlistViewModel : ViewModel
    {
        public WaitlistComponentDetailsViewModel? WaitlistComponentDetailsViewModel { get; }
        public WaitlistComponentListViewModel? WaitlistComponentListViewModel { get; }
        public SelectedWaitlistTaskStore? SelectedWaitlistTaskStore { get; }

        public WaitlistViewModel(SelectedWaitlistTaskStore? selectedWaitlistTaskStore)
        {
            try
            {
                WaitlistComponentDetailsViewModel = new WaitlistComponentDetailsViewModel(selectedWaitlistTaskStore);
                WaitlistComponentListViewModel = new WaitlistComponentListViewModel(selectedWaitlistTaskStore);
                SelectedWaitlistTaskStore = selectedWaitlistTaskStore;
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }
    }
}