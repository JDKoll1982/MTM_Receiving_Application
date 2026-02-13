using MTM_Waitlist_Application_2._0.Models;
using MTM_Waitlist_Application_2._0.Core;
using System.Reflection;

namespace MTM_Waitlist_Application_2._0.Stores
{
    public class SelectedWaitlistTaskStore
    {
        private WaitlistTask? _selectedWaitlistTask;

        public WaitlistTask? SelectedWaitlistTask
        {
            get
            {
                try
                {
                    return _selectedWaitlistTask;
                }
                catch (Exception ex)
                {
                    ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                    return null; // Return a default value or handle accordingly
                }
            }
            set
            {
                try
                {
                    _selectedWaitlistTask = value;
                    SelectedWaitlistTaskChanged?.Invoke();
                }
                catch (Exception ex)
                {
                    ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                }
            }
        }

        public event Action? SelectedWaitlistTaskChanged;
    }
}