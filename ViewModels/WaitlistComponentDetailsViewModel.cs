using MTM_Waitlist_Application_2._0.Core;
using MTM_Waitlist_Application_2._0.Stores;
using MTM_Waitlist_Application_2._0.Views;
using System.Reflection;

// Encapsulation Completed. (2024/07/30)

namespace MTM_Waitlist_Application_2._0.ViewModels
{
    public class WaitlistComponentDetailsViewModel : ViewModel
    {
        private readonly SelectedWaitlistTaskStore? _selectedWaitlistTaskStore;

        public WaitlistComponentDetails WaitlistComponentDetails { get; set; }

        public string? Id => _selectedWaitlistTaskStore?.SelectedWaitlistTask?.Id.ToString() ?? "";
        public string? WorkCenter => _selectedWaitlistTaskStore?.SelectedWaitlistTask?.WorkCenter ?? "Please Select a Task";
        public string? RequestType => _selectedWaitlistTaskStore?.SelectedWaitlistTask?.RequestType ?? "";
        public string? Request
        {
            get => _selectedWaitlistTaskStore?.SelectedWaitlistTask?.Request ?? "";
            set { }
        }
        public string? RequestPriority => _selectedWaitlistTaskStore?.SelectedWaitlistTask?.RequestPriority ?? "";
        public string? TimeRemaining => _selectedWaitlistTaskStore?.SelectedWaitlistTask?.TimeRemaining.ToString() ?? "";
        public string? Status
        {
            get => _selectedWaitlistTaskStore?.SelectedWaitlistTask?.Status ?? "";
            set { }
        }
        public string? MHandler
        {
            get => _selectedWaitlistTaskStore?.SelectedWaitlistTask?.MHandler ?? "";
            set { }
        }
        public string? RequestTime => _selectedWaitlistTaskStore?.SelectedWaitlistTask?.RequestTime.ToString() ?? "";
        public bool? IsOverdue => _selectedWaitlistTaskStore?.SelectedWaitlistTask?.IsOverdue;
        public TimeSpan? TimeRemainingValue => _selectedWaitlistTaskStore?.SelectedWaitlistTask?.TimeRemainingValue;
        public bool HasSeenTask => _selectedWaitlistTaskStore?.SelectedWaitlistTask != null;

        public WaitlistComponentDetailsViewModel(SelectedWaitlistTaskStore? selectedWaitlistTaskStore)
        {
            try
            {
                _selectedWaitlistTaskStore = selectedWaitlistTaskStore ?? throw new ArgumentNullException(nameof(selectedWaitlistTaskStore));
                _selectedWaitlistTaskStore.SelectedWaitlistTaskChanged += SelectedWaitlistTaskStore_SelectedWaitlistTaskChanged;
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        protected override void Dispose()
        {
            try
            {
                if (_selectedWaitlistTaskStore != null)
                {
                    _selectedWaitlistTaskStore.SelectedWaitlistTaskChanged -= SelectedWaitlistTaskStore_SelectedWaitlistTaskChanged;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public void SelectedWaitlistTaskStore_SelectedWaitlistTaskChanged()
        {
            try
            {
                OnPropertyChanged(nameof(Id));
                OnPropertyChanged(nameof(WorkCenter));
                OnPropertyChanged(nameof(RequestType));
                OnPropertyChanged(nameof(Request));
                OnPropertyChanged(nameof(RequestPriority));
                OnPropertyChanged(nameof(TimeRemaining));
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(MHandler));
                OnPropertyChanged(nameof(RequestTime));
                OnPropertyChanged(nameof(IsOverdue));
                OnPropertyChanged(nameof(TimeRemainingValue));
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public void ButtonEnabler(bool x)
        {
            try
            {
                if (x)
                {
                    WaitlistComponentDetails.RequestCancelButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }
    }
}
