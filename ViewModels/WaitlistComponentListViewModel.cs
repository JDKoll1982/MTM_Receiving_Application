using MTM_Waitlist_Application_2._0.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using MTM_Waitlist_Application_2._0.Core.Database_Classes;
using MTM_Waitlist_Application_2._0.Models;
using MTM_Waitlist_Application_2._0.Stores;
using MTM_Waitlist_Application_2._0.Views;
using MTM_Waitlist_Application_2._0.Windows.UserLogin;
using Application = System.Windows.Application;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;

// Encapsulation completed. (2024/07/30)

namespace MTM_Waitlist_Application_2._0.ViewModels
{
    public class WaitlistComponentListItemViewModel : ViewModel
    {
        public WaitlistTask WaitlistTask { get; } // Holds the WaitlistTask object associated with this view model.

        public string? WorkCenter => WaitlistTask.WorkCenter ?? ""; // Gets the WorkCenter property from the WaitlistTask, defaulting to an empty string if null.
        public string? RequestType => WaitlistTask.RequestType ?? ""; // Gets the RequestType property from the WaitlistTask, defaulting to an empty string if null.
        public string? Request => WaitlistTask.Request ?? ""; // Gets the Request property from the WaitlistTask, defaulting to an empty string if null.
        public string? RequestPriority => WaitlistTask.RequestPriority ?? ""; // Gets the RequestPriority property from the WaitlistTask, defaulting to an empty string if null.
        public string? TimeRemaining => WaitlistTask.TimeRemaining.ToString() ?? ""; // Gets the TimeRemaining property from the WaitlistTask as a string, defaulting to an empty string if null.
        public string? Status => WaitlistTask.Status ?? ""; // Gets the Status property from the WaitlistTask, defaulting to an empty string if null.
        public string? MHandler => WaitlistTask.MHandler ?? ""; // Gets the MHandler property from the WaitlistTask, defaulting to an empty string if null.
        public string? RequestTime => WaitlistTask.RequestTime.ToString() ?? ""; // Gets the RequestTime property from the WaitlistTask as a string, defaulting to an empty string if null.
        public bool? IsOverdue => WaitlistTask.IsOverdue; // Gets the IsOverdue property from the WaitlistTask.
        public TimeSpan? TimeRemainingValue => WaitlistTask.TimeRemainingValue; // Gets the TimeRemainingValue property from the WaitlistTask.

        public WaitlistComponentListItemViewModel(WaitlistTask waitlistTask)
        {
            this.WaitlistTask = waitlistTask; // Initializes the view model with a given WaitlistTask.
        }
    } // Represents a single item in the waitlist, encapsulating the details of a WaitlistTask.

    public class WaitlistComponentListViewModel : ViewModel
    {
        private SelectedWaitlistTaskStore _blankWlTaskStore = new SelectedWaitlistTaskStore(); // Holds a blank SelectedWaitlistTaskStore for default values.
        public SelectedWaitlistTaskStore? SelectedWaitlistTaskStore = new SelectedWaitlistTaskStore(); // Holds the currently selected SelectedWaitlistTaskStore.
        private ObservableCollection<WaitlistComponentListItemViewModel> _waitlistComponentList = new ObservableCollection<WaitlistComponentListItemViewModel>(); // Holds the collection of WaitlistComponentListItemViewModel objects.
        private DispatcherTimer _timer; // Timer to periodically update the waitlist.
        private WaitlistComponentListItemViewModel _selectedWlItem; // Holds the currently selected waitlist item.

        public WaitlistComponentListViewModel(WaitlistComponentList waitListComponentList)
        {
            try
            {
                this.WaitListComponentList = waitListComponentList; // Initializes the view model with a given WaitlistComponentList.
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message); // Handles exceptions and shows errors using ErrorHandler.
            }
        }

        public WaitlistComponentListViewModel(SelectedWaitlistTaskStore? selectedWaitlistTaskStore)
        {
            try
            {
                SelectedWaitlistTaskStore = selectedWaitlistTaskStore; // Initializes the view model with a given SelectedWaitlistTaskStore.

                if (_waitlistComponentList.Count == 0)
                {
                    UpdateWaitlistComponentListViewModel(SelectedWlItem); // Updates the waitlist if it is empty.
                }

                _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) }; // Sets up the timer to tick every second.
                _timer.Tick += Timer_Tick; // Subscribes to the Tick event.
                _timer.Start(); // Starts the timer.
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message); // Handles exceptions and shows errors using ErrorHandler.
            }
        }

        public IEnumerable<WaitlistComponentListItemViewModel> WaitlistComponentListItemViewModels => _waitlistComponentList; // Exposes the collection of WaitlistComponentListItemViewModel objects.

        public WaitlistComponentListItemViewModel SelectedWlItem
        {
            get => _selectedWlItem;
            set
            {
                try
                {
                    _selectedWlItem = value;
                    OnPropertyChanged(nameof(SelectedWlItem)); // Notifies property changes.
                    if (_selectedWlItem != null && SelectedWaitlistTaskStore != null)
                    {
                        SelectedWaitlistTaskStore.SelectedWaitlistTask = _selectedWlItem.WaitlistTask; // Updates the SelectedWaitlistTaskStore with the selected waitlist task.
                    }
                    else
                    {
                        SelectedWaitlistTaskStore.SelectedWaitlistTask = _blankWlTaskStore.SelectedWaitlistTask; // Sets the SelectedWaitlistTaskStore to a blank task if no item is selected.
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message); // Handles exceptions and shows errors using ErrorHandler.
                }
            }
        }

        public ObservableCollection<WaitlistComponentListItemViewModel> WaitlistComponentList
        {
            get => _waitlistComponentList;
            set
            {
                try
                {
                    _waitlistComponentList = value;
                    OnPropertyChanged(nameof(WaitlistComponentList)); // Notifies property changes.
                }
                catch (Exception ex)
                {
                    ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message); // Handles exceptions and shows errors using ErrorHandler.
                }
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                UpdateWaitlistComponentListViewModel(SelectedWlItem); // Refreshes the waitlist on each timer tick.
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message); // Handles exceptions and shows errors using ErrorHandler.
            }
        }

        public void UpdateWaitlistComponentListViewModel(WaitlistComponentListItemViewModel selectedWlItem)
        {
            try
            {
                var selectedItemId = selectedWlItem?.WaitlistTask.Id; // Stores the ID of the selected waitlist item.
                _waitlistComponentList.Clear(); // Clears the current waitlist.

                if (Application.Current != null)
                {
                    Window? mainWindow = Application.Current.MainWindow;
                    if (mainWindow != null)
                        mainWindow.Title = "MTM Waitlist Application | " + UserLogin.FullName + " | " + UserLogin.Role; // Updates the main window title with user information.
                }

                foreach (var item in SqlCommands.GetActiveWaitlist())
                {
                    var id = item.Id;
                    var workCenter = item.WorkCenter;
                    var requestType = item.RequestType;
                    var request = item.Request;
                    var requestPriority = item.RequestPriority;
                    var mHandler = item.MHandler;
                    var requestTime = item.RemainingTime;
                    TimeSpan timeRemaining = item.TimeRemaining;

                    _waitlistComponentList.Add(new WaitlistComponentListItemViewModel(new WaitlistTask(id, workCenter, requestType, request, requestPriority, timeRemaining, "", mHandler, requestTime))); // Adds each active waitlist item to the collection.
                }

                if (selectedItemId != null)
                {
                    SelectedWlItem = _waitlistComponentList.FirstOrDefault(item => item.WaitlistTask.Id == selectedItemId); // Sets the selected item based on the stored ID.
                }

                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(_waitlistComponentList);
                view.SortDescriptions.Add(new SortDescription("TimeRemainingValue", ListSortDirection.Ascending)); // Sorts the waitlist by TimeRemainingValue in ascending order.
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message); // Handles exceptions and shows errors using ErrorHandler.
            }
        }

        protected override void Dispose()
        {
            try
            {
                if (_timer != null)
                {
                    _timer.Tick -= Timer_Tick; // Unsubscribes from the Tick event.
                    _timer = null; // Disposes of the timer.
                }

                base.Dispose(); // Calls the base Dispose method to clean up resources.
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message); // Handles exceptions and shows errors using ErrorHandler.
            }
        }
    } // Manages a collection of WaitlistComponentListItemViewModel objects and handles the logic for updating and sorting the waitlist.
}