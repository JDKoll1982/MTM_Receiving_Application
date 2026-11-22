using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.UI;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Views;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// The Non-Inventoried Labels page as a shell action provider: every mode chip, Back, Next, Clear
/// All Labels and Help the shared strip draws comes from here.
/// </summary>
/// <remarks>
/// The page no longer draws a navigation row of its own. The verbs and the gates that decide
/// whether they run stay in this module, and the strip only renders the snapshot.
/// </remarks>
public partial class ViewModel_Dunnage_WorkFlowViewModel : IViewModel_ShellActionProvider
{
    private const string GuidedModeValue = "guided";
    private const string EditModeValue = "edit";
    private const string ImageSearchModeValue = "image-search";

    private Model_ShellActionBarState _shellActions = Model_ShellActionBarState.Empty;
    private string _activeMode = GuidedModeValue;
    private ViewModel_Dunnage_PartSelection? _partSelection;
    private ViewModel_Dunnage_QuantityEntry? _quantityEntry;
    private ViewModel_Dunnage_DetailsEntry? _detailsEntry;
    private ViewModel_Dunnage_Review? _review;
    private ViewModel_Dunnage_EditMode? _editMode;
    private Enum_DunnageWorkflowStep _imageSearchReturnStep = Enum_DunnageWorkflowStep.TypeSelection;
    private bool _isAdvanceInProgress;

    [ObservableProperty]
    private string _shellBackText = string.Empty;

    [ObservableProperty]
    private string _shellNextText = string.Empty;

    [ObservableProperty]
    private string _shellHelpText = string.Empty;

    [ObservableProperty]
    private string _shellClearAllLabelsText = string.Empty;

    [ObservableProperty]
    private string _shellReviewText = string.Empty;

    [ObservableProperty]
    private string _shellSaveText = string.Empty;

    [ObservableProperty]
    private string _shellGuidedText = string.Empty;

    [ObservableProperty]
    private string _shellEditText = string.Empty;

    [ObservableProperty]
    private string _shellImageSearchText = string.Empty;

    [ObservableProperty]
    private string _shellDefaultText = string.Empty;

    [ObservableProperty]
    private string _shellGuidedDefaultTooltipText = string.Empty;

    [ObservableProperty]
    private string _shellEditDefaultTooltipText = string.Empty;

    [ObservableProperty]
    private string _shellImageSearchDefaultTooltipText = string.Empty;

    /// <summary>
    /// Gets the snapshot the shared shell action bar renders.
    /// </summary>
    public Model_ShellActionBarState ShellActions
    {
        get => _shellActions;
        private set
        {
            if (ReferenceEquals(_shellActions, value))
            {
                return;
            }

            _shellActions = value;
            OnPropertyChanged(nameof(ShellActions));
        }
    }

    /// <summary>
    /// Set by the page's constructor so the Review verb can push the details text boxes into their
    /// view model before it validates them, without this view model taking a dependency on the view.
    /// </summary>
    public Action? RequestCommitDetailsInputs { get; set; }

    /// <summary>
    /// Set by the page's constructor so a mode change raises the same leave confirmation a page
    /// change does, rather than a second dialog with its own wording.
    /// </summary>
    public Func<string, Task<bool>>? RequestConfirmLeaveModule { get; set; }

    private string? CurrentDefaultMode => _sessionManager.CurrentSession?.User?.DefaultDunnageMode;

    /// <summary>
    /// Hands the hosted view models over so their gates and commands can fill the strip's slots,
    /// since only the page can see them.
    /// </summary>
    /// <param name="partSelection">The hosted Select Part view model.</param>
    /// <param name="quantityEntry">The hosted Enter Loads view model.</param>
    /// <param name="detailsEntry">The hosted Enter Details view model.</param>
    /// <param name="review">The hosted Review and Save view model.</param>
    /// <param name="editMode">The hosted Edit Mode view model.</param>
    public void AttachShellActionHosts(
        ViewModel_Dunnage_PartSelection partSelection,
        ViewModel_Dunnage_QuantityEntry quantityEntry,
        ViewModel_Dunnage_DetailsEntry detailsEntry,
        ViewModel_Dunnage_Review review,
        ViewModel_Dunnage_EditMode editMode
    )
    {
        ArgumentNullException.ThrowIfNull(partSelection);
        ArgumentNullException.ThrowIfNull(quantityEntry);
        ArgumentNullException.ThrowIfNull(detailsEntry);
        ArgumentNullException.ThrowIfNull(review);
        ArgumentNullException.ThrowIfNull(editMode);

        DetachHostPropertyChangedHandlers();

        _partSelection = partSelection;
        _quantityEntry = quantityEntry;
        _detailsEntry = detailsEntry;
        _review = review;
        _editMode = editMode;

        // Each hosted step decides for itself whether its own Next or Save may run, and announces
        // that change long after the step arrived, so the slot has to be re-read when it does.
        _partSelection.PropertyChanged += OnHostedViewModelPropertyChanged;
        _quantityEntry.PropertyChanged += OnHostedViewModelPropertyChanged;
        _detailsEntry.PropertyChanged += OnHostedViewModelPropertyChanged;
        _review.PropertyChanged += OnHostedViewModelPropertyChanged;
        _editMode.PropertyChanged += OnHostedViewModelPropertyChanged;

        RefreshShellActions();
    }

    /// <summary>
    /// Persists the signed-in user's default Non-Inventoried Labels mode. A Ctrl press on a mode
    /// chip is the only caller, so this never changes the active page.
    /// </summary>
    /// <param name="storedModeValue">guided, edit, image-search, or null to clear the default.</param>
    public void SetDefaultMode(string? storedModeValue) =>
        _ = SetDefaultModeAsync(storedModeValue);

    [RelayCommand]
    private async Task EnterGuidedModeAsync()
    {
        if (IsGuidedStep(_workflowService.CurrentStep))
        {
            // Already in the guided walk: pressing Guided must not restart it.
            return;
        }

        await SwitchModeAsync(
            GuidedModeValue,
            Enum_DunnageWorkflowStep.TypeSelection,
            _shellGuidedText
        );
    }

    [RelayCommand]
    private async Task EnterEditModeAsync()
    {
        if (_workflowService.CurrentStep == Enum_DunnageWorkflowStep.EditMode)
        {
            return;
        }

        await SwitchModeAsync(EditModeValue, Enum_DunnageWorkflowStep.EditMode, _shellEditText);
    }

    [RelayCommand]
    private async Task EnterImageSearchModeAsync()
    {
        var returnStep = _workflowService.CurrentStep;
        if (returnStep == Enum_DunnageWorkflowStep.ImagePartSearch)
        {
            return;
        }

        // A confirmation that clears unsaved work clears the session with it, so the step the
        // operator came from no longer holds anything to go back to.
        var hadUnsavedWork = _workflowService.HasUnsavedData();

        var switched = await SwitchModeAsync(
            ImageSearchModeValue,
            Enum_DunnageWorkflowStep.ImagePartSearch,
            _shellImageSearchText
        );

        if (switched)
        {
            _imageSearchReturnStep = hadUnsavedWork
                ? Enum_DunnageWorkflowStep.TypeSelection
                : returnStep;
        }
    }

    [RelayCommand]
    private async Task NextStepAsync()
    {
        if (_isAdvanceInProgress || _workflowService.IsNavigationLocked)
        {
            return;
        }

        switch (_workflowService.CurrentStep)
        {
            case Enum_DunnageWorkflowStep.TypeSelection:
                _isAdvanceInProgress = true;
                try
                {
                    await AdvanceAndReportAsync(await _workflowService.AdvanceToNextStepAsync());
                }
                finally
                {
                    _isAdvanceInProgress = false;
                }

                break;

            case Enum_DunnageWorkflowStep.PartSelection when _partSelection is not null:
                await RunCommandAsync(_partSelection.SelectPartCommand);
                break;

            case Enum_DunnageWorkflowStep.QuantityEntry when _quantityEntry is not null:
                await RunCommandAsync(_quantityEntry.GoNextCommand);
                break;

            case Enum_DunnageWorkflowStep.DetailsEntry:
                await ReviewAsync();
                break;

            case Enum_DunnageWorkflowStep.Review when _review is not null:
                await RunCommandAsync(_review.SaveAllCommand);
                break;

            case Enum_DunnageWorkflowStep.EditMode when _editMode is not null:
                await RunCommandAsync(_editMode.SaveAllCommand);
                break;
        }
    }

    [RelayCommand]
    private void PreviousStep()
    {
        switch (_workflowService.CurrentStep)
        {
            case Enum_DunnageWorkflowStep.PartSelection when _partSelection is not null:
                RunCommand(_partSelection.GoBackCommand);
                break;

            case Enum_DunnageWorkflowStep.QuantityEntry when _quantityEntry is not null:
                RunCommand(_quantityEntry.GoBackCommand);
                break;

            case Enum_DunnageWorkflowStep.DetailsEntry when _detailsEntry is not null:
                RunCommand(_detailsEntry.GoBackCommand);
                break;

            case Enum_DunnageWorkflowStep.Review:
                _workflowService.GoToStep(Enum_DunnageWorkflowStep.DetailsEntry);
                break;

            case Enum_DunnageWorkflowStep.ImagePartSearch:
                // Back returns to the guided step the operator came from, and to Select Type when
                // Image Search was the mode the module opened in.
                _workflowService.GoToStep(_imageSearchReturnStep);
                break;
        }
    }

    [RelayCommand(CanExecute = nameof(CanReview))]
    private async Task ReviewAsync()
    {
        if (_detailsEntry is null || _isAdvanceInProgress || _workflowService.IsNavigationLocked)
        {
            return;
        }

        _isAdvanceInProgress = true;

        try
        {
            RequestCommitDetailsInputs?.Invoke();

            if (!await EnsureDetailsLocationResolvedAsync())
            {
                return;
            }

            // If no PO was entered, give the operator a chance to supply a non-PO reference before
            // advancing. The dialog also persists commonly-used reasons.
            if (string.IsNullOrWhiteSpace(_workflowService.CurrentSession.PONumber))
            {
                var nonPoResult = await AskForNonPoReferenceAsync();
                if (nonPoResult is null)
                {
                    // Cancelled: stay on Enter Details so they can fill the PO or reconsider.
                    return;
                }

                _workflowService.CurrentSession.PONumber = nonPoResult;
                _detailsEntry.PoNumber = nonPoResult;
            }

            await AdvanceAndReportAsync(await _detailsEntry.SaveAndAdvanceAsync());
        }
        finally
        {
            _isAdvanceInProgress = false;
        }
    }

    private bool CanReview() => _detailsEntry?.CanProceedToNextStep == true;

    [RelayCommand]
    private async Task ShowShellHelpAsync()
    {
        await _shellHelpService.ShowContextualHelpAsync(_workflowService.CurrentStep);
    }

    /// <summary>
    /// Clears the active non-inventoried label queue. The strip binds a command with no parameter,
    /// so the plain and Shift activations each get their own wrapper.
    /// </summary>
    [RelayCommand]
    private async Task ClearAllLabelsAsync() => await ClearLabelsAsync(false);

    /// <summary>
    /// The wider clear, run when the strip's Clear All Labels control is activated with Shift held.
    /// </summary>
    [RelayCommand]
    private async Task ClearAllLabelsAndQueuedRowsAsync() => await ClearLabelsAsync(true);

    private Task ClearLabelsAsync(bool clearAllRows) =>
        ClearLabelDataCommand.CanExecute(clearAllRows)
            ? ClearLabelDataCommand.ExecuteAsync(clearAllRows)
            : Task.CompletedTask;

    private async Task<bool> SwitchModeAsync(
        string mode,
        Enum_DunnageWorkflowStep destination,
        string modeLabel
    )
    {
        if (RequestConfirmLeaveModule is not null)
        {
            var confirmed = await RequestConfirmLeaveModule(modeLabel);
            if (!confirmed)
            {
                return false;
            }
        }

        _activeMode = mode;
        _workflowService.GoToStep(destination);
        return true;
    }

    private async Task SetDefaultModeAsync(string? storedModeValue)
    {
        try
        {
            var currentUser = _sessionManager.CurrentSession?.User;
            if (currentUser is null)
            {
                return;
            }

            var mode = NormalizeDefaultMode(storedModeValue);
            var result = await _userPreferencesService.UpdateDefaultDunnageModeAsync(
                currentUser.WindowsUsername,
                mode
            );

            if (result.IsSuccess)
            {
                currentUser.DefaultDunnageMode = mode;
                RefreshShellActions();
            }
            else
            {
                await _errorHandler.ShowErrorDialogAsync(
                    "Save Error",
                    result.ErrorMessage,
                    Enum_ErrorSeverity.Error
                );
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Error,
                nameof(SetDefaultModeAsync),
                nameof(ViewModel_Dunnage_WorkFlowViewModel)
            );
        }
    }

    private static string? NormalizeDefaultMode(string? storedModeValue) =>
        storedModeValue?.Trim().ToLowerInvariant() switch
        {
            GuidedModeValue => GuidedModeValue,
            EditModeValue => EditModeValue,
            ImageSearchModeValue => ImageSearchModeValue,
            _ => null,
        };

    private static bool IsGuidedStep(Enum_DunnageWorkflowStep step) =>
        step
            is Enum_DunnageWorkflowStep.TypeSelection
                or Enum_DunnageWorkflowStep.PartSelection
                or Enum_DunnageWorkflowStep.QuantityEntry
                or Enum_DunnageWorkflowStep.DetailsEntry
                or Enum_DunnageWorkflowStep.Review;

    private void OnHostedViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        _dispatcherService.TryEnqueue(RefreshShellActions);

    private void DetachHostPropertyChangedHandlers()
    {
        if (_partSelection is not null)
        {
            _partSelection.PropertyChanged -= OnHostedViewModelPropertyChanged;
        }

        if (_quantityEntry is not null)
        {
            _quantityEntry.PropertyChanged -= OnHostedViewModelPropertyChanged;
        }

        if (_detailsEntry is not null)
        {
            _detailsEntry.PropertyChanged -= OnHostedViewModelPropertyChanged;
        }

        if (_review is not null)
        {
            _review.PropertyChanged -= OnHostedViewModelPropertyChanged;
        }

        if (_editMode is not null)
        {
            _editMode.PropertyChanged -= OnHostedViewModelPropertyChanged;
        }
    }

    /// <summary>
    /// Loads the strip's wording from settings, so a manager can reword it without a rebuild.
    /// </summary>
    private async Task InitializeShellTextAsync()
    {
        try
        {
            ShellBackText = await _dunnageSettings.GetStringAsync(
                DunnageSettingsKeys.UiText.WorkflowBack
            );
            ShellNextText = await _dunnageSettings.GetStringAsync(
                DunnageSettingsKeys.UiText.WorkflowNext
            );
            ShellHelpText = await _dunnageSettings.GetStringAsync(
                DunnageSettingsKeys.UiText.WorkflowHelp
            );
            ShellClearAllLabelsText = await _dunnageSettings.GetStringAsync(
                DunnageSettingsKeys.UiText.WorkflowClearAllLabels
            );
            ShellReviewText = await _dunnageSettings.GetStringAsync(
                DunnageSettingsKeys.UiText.WorkflowReview
            );
            ShellSaveText = await _dunnageSettings.GetStringAsync(
                DunnageSettingsKeys.UiText.WorkflowSave
            );
            ShellGuidedText = await _dunnageSettings.GetStringAsync(
                DunnageSettingsKeys.UiText.ShellGuided
            );
            ShellEditText = await _dunnageSettings.GetStringAsync(
                DunnageSettingsKeys.UiText.ShellEdit
            );
            ShellImageSearchText = await _dunnageSettings.GetStringAsync(
                DunnageSettingsKeys.UiText.ShellImageSearch
            );
            ShellDefaultText = await _dunnageSettings.GetStringAsync(
                DunnageSettingsKeys.UiText.ShellDefault
            );
            ShellGuidedDefaultTooltipText = await _dunnageSettings.GetStringAsync(
                DunnageSettingsKeys.UiText.ShellGuidedDefaultTooltip
            );
            ShellEditDefaultTooltipText = await _dunnageSettings.GetStringAsync(
                DunnageSettingsKeys.UiText.ShellEditDefaultTooltip
            );
            ShellImageSearchDefaultTooltipText = await _dunnageSettings.GetStringAsync(
                DunnageSettingsKeys.UiText.ShellImageSearchDefaultTooltip
            );

            RefreshShellActions();
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(InitializeShellTextAsync),
                nameof(ViewModel_Dunnage_WorkFlowViewModel)
            );
        }
    }

    private void RefreshShellActions() => ShellActions = BuildShellActions();

    private Model_ShellActionBarState BuildShellActions()
    {
        var step = _workflowService.CurrentStep;
        var activeMode = ResolveActiveMode(step);

        return new Model_ShellActionBarState
        {
            IsVisible = true,
            ShowModeGroup = true,
            Guided = CreateModeAction(
                GuidedModeValue,
                ShellGuidedText,
                ShellGuidedDefaultTooltipText,
                EnterGuidedModeCommand,
                canBeDefault: true,
                ShellDefaultText,
                isActive: activeMode == GuidedModeValue,
                showDividerAfter: true
            ),
            Manual = Model_ShellModeAction.Hidden,
            Edit = CreateModeAction(
                EditModeValue,
                ShellEditText,
                ShellEditDefaultTooltipText,
                EnterEditModeCommand,
                canBeDefault: true,
                ShellDefaultText,
                isActive: activeMode == EditModeValue,
                showDividerAfter: true
            ),
            Overlay = Model_ShellModeAction.Hidden,
            ImageSearch = CreateModeAction(
                ImageSearchModeValue,
                ShellImageSearchText,
                ShellImageSearchDefaultTooltipText,
                EnterImageSearchModeCommand,
                canBeDefault: true,
                ShellDefaultText,
                isActive: activeMode == ImageSearchModeValue,
                // Image Search is this module's last chip, so a trailing divider would hang off
                // the end of the group.
                showDividerAfter: false
            ),
            Back = BuildBackSlot(step),
            Next = BuildNextSlot(step),
            ClearLabelData = CreateSlot(
                isVisible: true,
                isEnabled: CanClearLabelData && CanAct,
                ShellClearAllLabelsText,
                ShellClearAllLabelsText,
                ClearAllLabelsCommand,
                shiftCommand: ClearAllLabelsAndQueuedRowsCommand
            ),
            Help = CreateSlot(
                isVisible: true,
                isEnabled: true,
                ShellHelpText,
                ShellHelpText,
                ShowShellHelpCommand
            ),
            IsGuidedDefault = CurrentDefaultMode == GuidedModeValue,
            IsManualDefault = false,
            IsEditDefault = CurrentDefaultMode == EditModeValue,
            IsImageSearchDefault = CurrentDefaultMode == ImageSearchModeValue,
        };
    }

    private bool CanAct => !_workflowService.IsNavigationLocked && !_isAdvanceInProgress;

    private Model_ShellActionSlot BuildBackSlot(Enum_DunnageWorkflowStep step)
    {
        switch (step)
        {
            case Enum_DunnageWorkflowStep.PartSelection when _partSelection is not null:
                return CreateSlot(
                    true,
                    CanAct,
                    ShellBackText,
                    ShellBackText,
                    _partSelection.GoBackCommand,
                    kind: Enum_ShellActionKind.Back
                );

            case Enum_DunnageWorkflowStep.QuantityEntry when _quantityEntry is not null:
                return CreateSlot(
                    true,
                    CanAct,
                    ShellBackText,
                    ShellBackText,
                    _quantityEntry.GoBackCommand,
                    kind: Enum_ShellActionKind.Back
                );

            case Enum_DunnageWorkflowStep.DetailsEntry when _detailsEntry is not null:
                return CreateSlot(
                    true,
                    CanAct,
                    ShellBackText,
                    ShellBackText,
                    _detailsEntry.GoBackCommand,
                    kind: Enum_ShellActionKind.Back
                );

            case Enum_DunnageWorkflowStep.Review:
            case Enum_DunnageWorkflowStep.ImagePartSearch:
                return CreateSlot(
                    true,
                    CanAct,
                    ShellBackText,
                    ShellBackText,
                    PreviousStepCommand,
                    kind: Enum_ShellActionKind.Back
                );

            default:
                // Select Type and Edit Mode have no way back, so the control is absent rather
                // than disabled.
                return Model_ShellActionSlot.Hidden;
        }
    }

    private Model_ShellActionSlot BuildNextSlot(Enum_DunnageWorkflowStep step)
    {
        switch (step)
        {
            case Enum_DunnageWorkflowStep.TypeSelection:
                return CreateSlot(
                    true,
                    CanAct && _workflowService.CurrentSession.SelectedTypeId > 0,
                    ShellNextText,
                    ShellNextText,
                    NextStepCommand
                );

            case Enum_DunnageWorkflowStep.PartSelection when _partSelection is not null:
                return CreateSlot(
                    true,
                    CanAct && _partSelection.SelectPartCommand.CanExecute(null),
                    ShellNextText,
                    ShellNextText,
                    _partSelection.SelectPartCommand
                );

            case Enum_DunnageWorkflowStep.QuantityEntry when _quantityEntry is not null:
                return CreateSlot(
                    true,
                    CanAct && _quantityEntry.GoNextCommand.CanExecute(null),
                    ShellNextText,
                    ShellNextText,
                    _quantityEntry.GoNextCommand
                );

            case Enum_DunnageWorkflowStep.DetailsEntry:
                return CreateSlot(
                    true,
                    CanAct && CanReview(),
                    ShellReviewText,
                    ShellReviewText,
                    ReviewCommand
                );

            case Enum_DunnageWorkflowStep.Review when _review is not null:
                return CreateSlot(
                    true,
                    CanAct && _review.CanSave,
                    ShellSaveText,
                    ShellSaveText,
                    _review.SaveAllCommand,
                    kind: Enum_ShellActionKind.Save
                );

            case Enum_DunnageWorkflowStep.EditMode when _editMode is not null:
                return CreateSlot(
                    true,
                    CanAct && _editMode.SaveAllCommand.CanExecute(null),
                    ShellSaveText,
                    ShellSaveText,
                    _editMode.SaveAllCommand,
                    kind: Enum_ShellActionKind.Save
                );

            default:
                // Image Search offers no Next.
                return Model_ShellActionSlot.Hidden;
        }
    }

    private string ResolveActiveMode(Enum_DunnageWorkflowStep step)
    {
        switch (step)
        {
            case Enum_DunnageWorkflowStep.EditMode:
                _activeMode = EditModeValue;
                break;
            case Enum_DunnageWorkflowStep.ImagePartSearch:
                _activeMode = ImageSearchModeValue;
                break;
            case Enum_DunnageWorkflowStep.TypeSelection:
            case Enum_DunnageWorkflowStep.PartSelection:
            case Enum_DunnageWorkflowStep.QuantityEntry:
            case Enum_DunnageWorkflowStep.DetailsEntry:
            case Enum_DunnageWorkflowStep.Review:
                _activeMode = GuidedModeValue;
                break;
        }

        return _activeMode;
    }

    private async Task<bool> EnsureDetailsLocationResolvedAsync()
    {
        if (_detailsEntry is null)
        {
            return false;
        }

        var validation = await _detailsEntry.ValidateLocationAsync();
        if (validation.IsValid)
        {
            return true;
        }

        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot is null)
        {
            _logger.LogWarning("XamlRoot is null, proceeding without location confirmation");
            return true;
        }

        if (string.IsNullOrWhiteSpace(_detailsEntry.Location) is false)
        {
            var warningDialog = new ContentDialog
            {
                Title = "Location Not Verified",
                Content =
                    $"{validation.Message}\n\nThe entered location will still be saved as-is.",
                CloseButtonText = "Continue",
                XamlRoot = xamlRoot,
            };
            MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
                warningDialog,
                xamlRoot
            );
            await warningDialog.ShowAsync();
            return true;
        }

        var errorDialog = new ContentDialog
        {
            Title = "Finish This Step First",
            Content = validation.Message,
            CloseButtonText = "OK",
            XamlRoot = xamlRoot,
        };
        MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
            errorDialog,
            xamlRoot
        );
        await errorDialog.ShowAsync();
        return false;
    }

    private async Task<string?> AskForNonPoReferenceAsync()
    {
        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot is null)
        {
            _logger.LogWarning("XamlRoot is null, advancing without a non-PO reference");
            return string.Empty;
        }

        var dialog = new View_Dunnage_Dialog_NonPOEntry(
            _workflowService.CurrentSession.SelectedPart?.PartId
        )
        {
            XamlRoot = xamlRoot,
        };

        await dialog.ShowAsync();
        return dialog.Result;
    }

    private async Task AdvanceAndReportAsync(Model_WorkflowStepResult result)
    {
        if (result.IsSuccess)
        {
            if (string.IsNullOrWhiteSpace(result.WarningMessage) is false)
            {
                await _errorHandler.HandleErrorAsync(
                    result.WarningMessage,
                    Enum_ErrorSeverity.Warning
                );
            }

            return;
        }

        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot is null)
        {
            await _errorHandler.HandleErrorAsync(
                result.ErrorMessage,
                Enum_ErrorSeverity.Warning
            );
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Finish This Step First",
            Content = result.ErrorMessage,
            CloseButtonText = "OK",
            XamlRoot = xamlRoot,
        };
        MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
            dialog,
            xamlRoot
        );
        await dialog.ShowAsync();
    }

    private static void RunCommand(System.Windows.Input.ICommand? command)
    {
        if (command?.CanExecute(null) == true)
        {
            command.Execute(null);
        }
    }

    private static async Task RunCommandAsync(System.Windows.Input.ICommand? command)
    {
        if (command?.CanExecute(null) != true)
        {
            return;
        }

        if (command is IAsyncRelayCommand asyncCommand)
        {
            await asyncCommand.ExecuteAsync(null);
            return;
        }

        command.Execute(null);
    }

    private static Model_ShellActionSlot CreateSlot(
        bool isVisible,
        bool isEnabled,
        string label,
        string tooltip,
        System.Windows.Input.ICommand? command,
        bool showDividerAfter = false,
        Enum_ShellActionKind kind = Enum_ShellActionKind.Plain,
        System.Windows.Input.ICommand? shiftCommand = null
    ) =>
        new()
        {
            IsVisible = isVisible,
            IsEnabled = isEnabled,
            Label = label,
            Tooltip = tooltip,
            AccessibilityName = label,
            Command = command,
            ShiftCommand = shiftCommand,
            ShowDividerAfter = showDividerAfter,
            Kind = kind,
        };

    private static Model_ShellModeAction CreateModeAction(
        string mode,
        string label,
        string defaultTooltip,
        System.Windows.Input.ICommand? command,
        bool canBeDefault,
        string defaultBadge,
        bool isActive,
        bool showDividerAfter
    ) =>
        new()
        {
            Mode = mode,
            Label = label,
            Tooltip = label,
            AccessibilityName = label,
            Command = command,
            CanBeDefault = canBeDefault,
            DefaultBadge = canBeDefault ? defaultBadge : string.Empty,
            DefaultTooltip = defaultTooltip,
            ShowDividerAfter = showDividerAfter,
            IsActive = isActive,
        };
}
