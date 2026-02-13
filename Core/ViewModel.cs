using System.ComponentModel;
using System.Reflection;
using System.Windows;
using MTM_Waitlist_Application_2._0.Views;
using MessageBox = System.Windows.MessageBox;

// Encapsulation Complete. (2024/07/30)

namespace MTM_Waitlist_Application_2._0.Core
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        public WaitlistComponentList WaitListComponentList { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string? propertyName = null)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        protected virtual void Dispose()
        {
            try
            {
                // Dispose logic here
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }
    }
}