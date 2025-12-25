using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.ViewModels.Shared;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Enums;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Collections.Generic;
using System.IO;
using MTM_Receiving_Application.Models;

namespace MTM_Receiving_Application.ViewModels.Receiving
{
    /// <summary>
    /// ViewModel for Edit Mode - allows editing existing loads but not adding new ones.
    /// </summary>
    public partial class EditModeViewModel : BaseViewModel
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_MySQL_Receiving _mysqlService;
        private readonly IService_CSVWriter _csvWriter;

        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads;

        [ObservableProperty]
        private Model_ReceivingLoad? _selectedLoad;

        [ObservableProperty]
        private DataSourceType _currentDataSource = DataSourceType.Memory;

        [ObservableProperty]
        private string _selectAllButtonText = "Select All";

        private string? _currentCsvPath;
        private readonly List<Model_ReceivingLoad> _deletedLoads = new();

        public ObservableCollection<Enum_PackageType> PackageTypes { get; } = new(Enum.GetValues<Enum_PackageType>());

        public EditModeViewModel(
            IService_ReceivingWorkflow workflowService,
            IService_MySQL_Receiving mysqlService,
            IService_CSVWriter csvWriter,
            IService_ErrorHandler errorHandler,
            ILoggingService logger)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService;
            _mysqlService = mysqlService;
            _csvWriter = csvWriter;
            _loads = new ObservableCollection<Model_ReceivingLoad>();
            _loads.CollectionChanged += Loads_CollectionChanged;
            
            _logger.LogInfo("Edit Mode initialized");
        }

        private void Loads_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Model_ReceivingLoad item in e.NewItems)
                {
                    item.PropertyChanged += Load_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (Model_ReceivingLoad item in e.OldItems)
                {
                    item.PropertyChanged -= Load_PropertyChanged;
                }
            }
            
            NotifyCommands();
        }

        private void Load_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model_ReceivingLoad.IsSelected))
            {
                RemoveRowCommand.NotifyCanExecuteChanged();
            }
        }

        private void NotifyCommands()
        {
            SaveCommand.NotifyCanExecuteChanged();
            RemoveRowCommand.NotifyCanExecuteChanged();
            SelectAllCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        private async Task LoadFromCurrentMemoryAsync()
        {
            try
            {
                _logger.LogInfo("Loading data from current memory");
                IsBusy = true;
                StatusMessage = "Loading from current session...";
                _deletedLoads.Clear();

                var currentLoads = _workflowService.CurrentSession.Loads;
                if (currentLoads.Count == 0)
                {
                    await _errorHandler.HandleErrorAsync(
                        "No data in current session. Please use Manual Entry mode to create new loads.",
                        Enum_ErrorSeverity.Warning);
                    return;
                }

                Loads.Clear();
                foreach (var load in currentLoads)
                {
                    Loads.Add(load);
                }

                StatusMessage = $"Loaded {Loads.Count} loads from current session";
                _logger.LogInfo($"Successfully loaded {Loads.Count} loads from current memory");
                CurrentDataSource = DataSourceType.Memory;
                SelectAllButtonText = "Select All";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load from current memory: {ex.Message}");
                await _errorHandler.HandleErrorAsync("Failed to load data from current session", Enum_ErrorSeverity.Error, ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task LoadFromCurrentLabelsAsync()
        {
            try
            {
                _logger.LogInfo("User initiated Current Labels (CSV) load");
                IsBusy = true;
                StatusMessage = "Checking for existing label files...";

                // Try to load from default locations first
                if (await TryLoadFromDefaultCsvAsync())
                {
                    return;
                }

                // If we get here, no file was found or loaded
                await _errorHandler.ShowErrorDialogAsync(
                    "No Labels Found", 
                    "Could not find any current label files in the default locations.", 
                    Enum_ErrorSeverity.Warning);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load from labels: {ex.Message}");
                await _errorHandler.HandleErrorAsync("Failed to load data from label file", Enum_ErrorSeverity.Error, ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<bool> TryLoadFromDefaultCsvAsync()
        {
            // Try local path first
            string localPath = _csvWriter.GetLocalCSVPath();
            if (File.Exists(localPath))
            {
                try 
                {
                    _logger.LogInfo($"Attempting to load from local CSV: {localPath}");
                    var loadedData = await _csvWriter.ReadFromCSVAsync(localPath);
                    
                    if (loadedData.Count > 0)
                    {
                        _deletedLoads.Clear();
                        Loads.Clear();
                        foreach (var load in loadedData)
                        {
                            Loads.Add(load);
                            _workflowService.CurrentSession.Loads.Add(load);
                        }
                        StatusMessage = $"Loaded {Loads.Count} loads from local labels";
                        _logger.LogInfo($"Successfully loaded {Loads.Count} loads from local labels");
                        CurrentDataSource = DataSourceType.CurrentLabels;
                        _currentCsvPath = localPath;
                        SelectAllButtonText = "Select All";
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to load from local labels: {ex.Message}");
                }
            }

            // Try network path
            try
            {
                string networkPath = _csvWriter.GetNetworkCSVPath();
                if (File.Exists(networkPath))
                {
                    _logger.LogInfo($"Attempting to load from network labels: {networkPath}");
                    var loadedData = await _csvWriter.ReadFromCSVAsync(networkPath);
                    
                    if (loadedData.Count > 0)
                    {
                        _deletedLoads.Clear();
                        Loads.Clear();
                        foreach (var load in loadedData)
                        {
                            Loads.Add(load);
                            _workflowService.CurrentSession.Loads.Add(load);
                        }
                        StatusMessage = $"Loaded {Loads.Count} loads from network labels";
                        _logger.LogInfo($"Successfully loaded {Loads.Count} loads from network labels");
                        CurrentDataSource = DataSourceType.CurrentLabels;
                        _currentCsvPath = networkPath;
                        SelectAllButtonText = "Select All";
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to load from network labels: {ex.Message}");
            }

            return false;
        }

        [RelayCommand]
        private async Task LoadFromHistoryAsync()
        {
            try
            {
                _logger.LogInfo("User initiated history load");
                IsBusy = true;
                StatusMessage = "Loading from history...";

                // Get date range from user
                var endDate = DateTime.Now;
                var startDate = endDate.AddMonths(-1); // Default to last month

                _logger.LogInfo($"Loading receiving history from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
                var result = await _mysqlService.GetAllReceivingLoadsAsync(startDate, endDate);

                if (!result.IsSuccess)
                {
                    await _errorHandler.HandleErrorAsync(
                        $"Failed to load from history: {result.ErrorMessage}",
                        Enum_ErrorSeverity.Error);
                    return;
                }

                if (result.Data == null || result.Data.Count == 0)
                {
                    await _errorHandler.HandleErrorAsync(
                        "No receiving records found in the specified date range.",
                        Enum_ErrorSeverity.Warning);
                    return;
                }

                _deletedLoads.Clear();
                Loads.Clear();
                foreach (var load in result.Data)
                {
                    Loads.Add(load);
                    _workflowService.CurrentSession.Loads.Add(load);
                }

                StatusMessage = $"Loaded {Loads.Count} loads from history";
                _logger.LogInfo($"Successfully loaded {Loads.Count} loads from history");
                CurrentDataSource = DataSourceType.History;
                SelectAllButtonText = "Select All";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load from history: {ex.Message}");
                await _errorHandler.HandleErrorAsync("Failed to load data from history", Enum_ErrorSeverity.Error, ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanSelectAll))]
        private void SelectAll()
        {
            // If any are unselected, we select all. Otherwise (all are selected), we deselect all.
            bool anyUnselected = Loads.Any(l => !l.IsSelected);
            
            if (anyUnselected)
            {
                foreach (var load in Loads) load.IsSelected = true;
                SelectAllButtonText = "Deselect All";
            }
            else
            {
                foreach (var load in Loads) load.IsSelected = false;
                SelectAllButtonText = "Select All";
            }
        }

        private bool CanSelectAll() => Loads.Count > 0;

        [RelayCommand(CanExecute = nameof(CanRemoveRow))]
        private void RemoveRow()
        {
            var selectedLoads = Loads.Where(l => l.IsSelected).ToList();
            
            if (selectedLoads.Any())
            {
                _logger.LogInfo($"Removing {selectedLoads.Count} selected loads");
                foreach (var load in selectedLoads)
                {
                    _deletedLoads.Add(load);
                    _workflowService.CurrentSession.Loads.Remove(load);
                    Loads.Remove(load);
                }
            }
            else if (SelectedLoad != null)
            {
                _logger.LogInfo($"Removing load {SelectedLoad.LoadNumber} (Part ID: {SelectedLoad.PartID})");
                _deletedLoads.Add(SelectedLoad);
                _workflowService.CurrentSession.Loads.Remove(SelectedLoad);
                Loads.Remove(SelectedLoad);
            }
            else
            {
                _logger.LogWarning("RemoveRow called with no selected load(s)");
            }
            SaveCommand.NotifyCanExecuteChanged(); // Update save command state after removal
        }

        private bool CanRemoveRow() => Loads.Any(l => l.IsSelected);

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsync()
        {
            try
            {
                _logger.LogInfo($"Validating and saving {Loads.Count} loads from edit mode");
                IsBusy = true;
                StatusMessage = "Validating loads...";

                // Validate all loads
                var validationErrors = ValidateLoads();
                if (validationErrors.Any())
                {
                    var errorMessage = string.Join("\n", validationErrors);
                    _logger.LogWarning($"Edit mode validation failed: {validationErrors.Count} errors");
                    await _errorHandler.HandleErrorAsync(
                        $"Validation failed:\n{errorMessage}", 
                        Enum_ErrorSeverity.Warning);
                    return;
                }

                StatusMessage = "Saving data...";
                
                switch (CurrentDataSource)
                {
                    case DataSourceType.Memory:
                        // For memory, we proceed with the standard workflow which will eventually save to DB/CSV
                        // as new records.
                        await _workflowService.AdvanceToNextStepAsync();
                        break;

                    case DataSourceType.CurrentLabels:
                        if (string.IsNullOrEmpty(_currentCsvPath))
                        {
                            await _errorHandler.HandleErrorAsync("No label file path available for saving.", Enum_ErrorSeverity.Error);
                            return;
                        }
                        
                        _logger.LogInfo($"Overwriting label file: {_currentCsvPath}");
                        await _csvWriter.WriteToFileAsync(_currentCsvPath, Loads.ToList(), append: false);
                        StatusMessage = "Label file updated successfully";
                        await _errorHandler.ShowErrorDialogAsync("Success", "Label file updated successfully.", Enum_ErrorSeverity.Info);
                        break;

                    case DataSourceType.History:
                        _logger.LogInfo("Updating history records");
                        int deleted = 0;
                        if (_deletedLoads.Count > 0)
                        {
                            _logger.LogInfo($"Deleting {_deletedLoads.Count} removed records");
                            deleted = await _mysqlService.DeleteReceivingLoadsAsync(_deletedLoads);
                        }

                        int updated = 0;
                        if (Loads.Count > 0)
                        {
                            updated = await _mysqlService.UpdateReceivingLoadsAsync(Loads.ToList());
                        }
                        
                        StatusMessage = $"History updated ({updated} updated, {deleted} deleted)";
                        await _errorHandler.ShowErrorDialogAsync("Success", $"History updated successfully.\n{updated} records updated.\n{deleted} records deleted.", Enum_ErrorSeverity.Info);
                        break;
                }

                _logger.LogInfo($"Edit mode save completed successfully for source: {CurrentDataSource}");
                _deletedLoads.Clear(); // Clear deleted list after successful save
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save edit mode data: {ex.Message}");
                await _errorHandler.HandleErrorAsync("Failed to save receiving data", Enum_ErrorSeverity.Critical, ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanSave()
        {
            // Can save if there are loads OR if we have deleted loads (to save the deletions)
            return Loads.Count > 0 || _deletedLoads.Count > 0;
        }

        [RelayCommand]
        private async Task ReturnToModeSelectionAsync()
        {
            if (App.MainWindow?.Content?.XamlRoot == null)
            {
                _logger.LogError("Cannot show dialog: XamlRoot is null");
                await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error);
                return;
            }

            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "Change Mode?",
                Content = "Returning to mode selection will clear all current work in progress. This cannot be undone. Are you sure?",
                PrimaryButtonText = "Yes, Change Mode",
                CloseButtonText = "Cancel",
                DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            var result = await dialog.ShowAsync().AsTask();
            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                try
                {
                    _logger.LogInfo("User confirmed return to mode selection, resetting workflow");
                    // Reset workflow and return to mode selection
                    await _workflowService.ResetWorkflowAsync();
                    _workflowService.GoToStep(WorkflowStep.ModeSelection);
                    // The ReceivingWorkflowViewModel will handle the visibility update through StepChanged event
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to reset workflow: {ex.Message}");
                    await _errorHandler.HandleErrorAsync("Failed to return to mode selection", Enum_ErrorSeverity.Error, ex);
                }
            }
            else
            {
                _logger.LogInfo("User cancelled return to mode selection");
            }
        }

        private System.Collections.Generic.List<string> ValidateLoads()
        {
            var errors = new System.Collections.Generic.List<string>();

            // Allow empty loads if we have deleted items (user wants to delete everything)
            if (Loads.Count == 0 && _deletedLoads.Count == 0)
            {
                errors.Add("No loads to save");
                return errors;
            }

            foreach (var load in Loads)
            {
                if (string.IsNullOrWhiteSpace(load.PartID))
                {
                    errors.Add($"Load #{load.LoadNumber}: Part ID is required");
                }

                if (load.WeightQuantity <= 0)
                {
                    errors.Add($"Load #{load.LoadNumber}: Weight/Quantity must be greater than zero");
                }

                if (load.PackagesPerLoad <= 0)
                {
                    errors.Add($"Load #{load.LoadNumber}: Packages per load must be greater than zero");
                }
            }

            return errors;
        }
    }
}
