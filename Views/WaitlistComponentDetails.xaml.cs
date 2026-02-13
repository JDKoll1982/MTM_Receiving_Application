using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using MTM_Waitlist_Application_2._0.Core;
using MTM_Waitlist_Application_2._0.Core.Database_Classes;
using MTM_Waitlist_Application_2._0.Stores;
using MTM_Waitlist_Application_2._0.ViewModels;
using MTM_Waitlist_Application_2._0.Windows.UserLogin;
using MessageBox = System.Windows.MessageBox;

// Encapsulation Completed. (2024/07/30)

namespace MTM_Waitlist_Application_2._0.Views
{
    public partial class WaitlistComponentDetails : System.Windows.Controls.UserControl
    {
        private readonly WaitlistComponentListViewModel? _waitlistComponentListViewModel;
        private readonly WaitlistComponentListItemViewModel? _selectedWlItem;
        private readonly SelectedWaitlistTaskStore? _selectedWaitlistTaskStore;
        private DispatcherTimer _timer;

        public int? Id
        {
            get
            {
                try
                {
                    return _selectedWaitlistTaskStore?.SelectedWaitlistTask?.Id;
                }
                catch (Exception ex)
                {
                    ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                    return null;
                }
            }
        }

        public WaitlistComponentDetails()
        {
            try
            {
                InitializeComponent();
                _waitlistComponentListViewModel = new WaitlistComponentListViewModel(_selectedWaitlistTaskStore!);

                _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(.1) };
                _timer.Tick += Timer_Tick;
                _timer.Start();
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        private void TakeRequestButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UserLogin.Role != "Operator")
                {
                    if (int.TryParse(RequestID.Text, out int id))
                    {
                        SqlCommands.AcceptTask(id);
                        _waitlistComponentListViewModel.UpdateWaitlistComponentListViewModel(_selectedWlItem);
                    }
                    else
                    {
                        MessageBox.Show("Please select a task to take.");
                    }
                }
                else
                {
                    MessageBox.Show("You do not have permission to take requests.");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        private void RequestCancelRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.TryParse(RequestID.Text, out int id))
                {
                    SqlCommands.CancelTask(id);
                    _waitlistComponentListViewModel.UpdateWaitlistComponentListViewModel(_selectedWlItem);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        private void RequestCompleteRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Action_RemoveTask(sender, e);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        private void Action_RemoveTask(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.TryParse(RequestID.Text, out int id))
                {
                    string input = Request.Text;
                    string trimString = "Pickup Finished Goods. Part Number: ";
                    string result = TrimStartString(input, trimString);

                    string input2 = result;
                    string trimString2 = " - Take To NCM.";
                    string result2 = TrimEndString(input2, trimString2);


                    SqlCommands.CompleteTask(id, Request.Text, result2, WorkCenter.Text, MHandler.Text, DateTime.Now);

                    _waitlistComponentListViewModel.UpdateWaitlistComponentListViewModel(_selectedWlItem);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public static string TrimStartString(string input, string trimString)
        {
            while (input.StartsWith(trimString))
            {
                input = input.Substring(trimString.Length);
            }
            return input;
        }

        public static string TrimEndString(string input, string trimString)
        {
            while (input.EndsWith(trimString))
            {
                input = input.Substring(0, input.Length - trimString.Length);
            }
            return input;
        }

        public void ButtonEnabler(bool x)
        {
            try
            {
                TakeRequestButton.IsEnabled = x;
                RequestCancelButton.IsEnabled = x;
                EditRequestButton.IsEnabled = x;
                RequestCompleteButton.IsEnabled = x;
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (WorkCenter.Text == "Please Select a Task")
                {
                    ButtonEnabler(false);
                }
                else
                {
                    ButtonEnabler(true);
                }

                RequestCompleteButton.IsEnabled = MHandler.Text != "";
                TakeRequestButton.IsEnabled = MHandler.Text == "" && WorkCenter.Text != "Please Select a Task";
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }
    }
}
