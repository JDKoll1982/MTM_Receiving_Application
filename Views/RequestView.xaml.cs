using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using MTM_Waitlist_Application_2._0.Core;
using MTM_Waitlist_Application_2._0.Core.Database_Classes;
using MTM_Waitlist_Application_2._0.ViewModels;
using MTM_Waitlist_Application_2._0.Windows.Main_Window;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

// Encapsulation Completed. (2024/07/30)

namespace MTM_Waitlist_Application_2._0.Views
{
    public partial class RequestView
    {
        public RequestView()
        {
            try
            {
                InitializeComponent();
                LoadComboBoxData();
                DataContext = new RequestViewModel();
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public void LoadComboBoxData()
        {
            try
            {
                var sqlCommands = new SqlCommands();

                CBWorkCenter.ItemsSource = new ObservableCollection<string>(
                    sqlCommands.ComboBoxFiller("work_center").Where(item => !string.IsNullOrEmpty(item))
                );
                CBWorkCenter.SelectedIndex = 0;

                CBRequestType.ItemsSource = new ObservableCollection<string>(
                    sqlCommands.ComboBoxFiller("request_type").Where(item => !string.IsNullOrEmpty(item))
                );
                CBRequestType.SelectedIndex = 0;

                CBRequestPriority.ItemsSource = new ObservableCollection<string>(
                    sqlCommands.ComboBoxFiller("priority").Where(item => !string.IsNullOrEmpty(item))
                );
                CBRequestPriority.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public TimeSpan CalculateTimeRemaining(string? dropDownText, TimeSpan timeRemaining)
        {
            try
            {
                DateTime timeOnly = new DateTime(DateTime.Now.TimeOfDay.Ticks);
                DateTime x;
                int minutes;

                switch (dropDownText)
                {
                    case "Low (Double the Time)":
                        minutes = timeRemaining.Minutes * 2;
                        break;
                    case "Medium (Default)":
                        minutes = timeRemaining.Minutes;
                        break;
                    case "High (Half the Time)":
                        minutes = timeRemaining.Minutes / 2;
                        break;
                    case "Urgent (No Time)":
                        minutes = 0;
                        break;
                    default:
                        minutes = timeRemaining.Minutes;
                        break;
                }

                x = timeOnly.AddMinutes(minutes);
                AllocatedTime.Content = $"{minutes} Minutes ( {x:hh:mm tt} )";
                return new TimeSpan(0, minutes, 0);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                return new TimeSpan(0, 0, 0);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedWorkCenter = CBWorkCenter.SelectedItem.ToString();
                var selectedRequestType = CBRequestType.SelectedItem.ToString();
                var selectedRequestPriority = CBRequestPriority.SelectedItem.ToString();

                if (selectedRequestPriority == "Urgent (No Time)")
                {
                    MessageBoxResult result = MessageBox.Show(
                        "Are you sure you want to submit this request as Urgent? Production Lead will be Notified!",
                        "Urgent Request",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning
                    );

                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                var newWaitlistData = new SqlCommands.GetActiveWaitlistData
                {
                    WorkCenter = selectedWorkCenter,
                    RequestType = selectedRequestType,
                    RequestPriority = selectedRequestPriority,
                    Request = new TextRange(RTBDescription.Document.ContentStart, RTBDescription.Document.ContentEnd).Text,
                    MHandler = "",
                    RemainingTime = DateTime.Now,
                    TimeRemaining = CalculateTimeRemaining(selectedRequestPriority, SqlCommands.GetTimeRemaining(selectedRequestType))
                };

                SqlCommands.AddToWaitlist(newWaitlistData);

                var mainWindow = Application.Current.MainWindow as MainWindow;
                var mainWindowViewModel = mainWindow?.DataContext as MainWindowViewModel;
                mainWindowViewModel?.NavigateToWaitlist.Execute(null);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        private void AllocatedTime_Change(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (CBRequestPriority.SelectedItem != null)
                {
                    var selectedRequestType = CBRequestType.SelectedItem?.ToString();
                    var selectedRequestPriority = CBRequestPriority.SelectedItem?.ToString();

                    if (selectedRequestType != null && selectedRequestPriority != null)
                    {
                        var timeRemaining = SqlCommands.GetTimeRemaining(selectedRequestType);
                        CalculateTimeRemaining(selectedRequestPriority, timeRemaining);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }
    }
}