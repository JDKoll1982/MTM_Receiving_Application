using System.Reflection;
using MTM_Waitlist_Application_2._0.Core;

// Encapsulation completed. (2024/07/30)

namespace MTM_Waitlist_Application_2._0.ViewModels
{
    public class MessengerViewModel : ViewModel
    {
        public MessengerViewModel()
        {
            try
            {
                // Your initialization code here
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }
    }
}