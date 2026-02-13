using MTM_Waitlist_Application_2._0.Core;
using MTM_Waitlist_Application_2._0.ViewModels;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using MTM_Waitlist_Application_2._0.WinForms.JobDetails;
using MessageBox = System.Windows.MessageBox;

// Encapsulation Complete. (2024/07/30)

namespace MTM_Waitlist_Application_2._0.Views
{
    public partial class WaitlistComponentList
    {
        public WaitlistComponentList()
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