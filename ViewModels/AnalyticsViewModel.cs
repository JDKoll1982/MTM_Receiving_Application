using System.Reflection;
using MTM_Waitlist_Application_2._0.Core;

namespace MTM_Waitlist_Application_2._0.ViewModels
{
    public class AnalyticsViewModel : ViewModel
    {
        public AnalyticsViewModel()
        {
            try
            {

            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }
    }
}