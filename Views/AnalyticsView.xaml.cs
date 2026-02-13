using MTM_Waitlist_Application_2._0.Core;
using System.Reflection;

// Encapsulation Complete (2024/07/30)

namespace MTM_Waitlist_Application_2._0.Views
{
    public partial class AnalyticsView
    {
        public AnalyticsView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }
    }
}