using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_OutsideService.Contracts;
using MTM_Receiving_Application.Module_OutsideService.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_OutsideService.ViewModels;

/// <summary>
/// Setup and completion ViewModel for a selected Outside Service line.
/// </summary>
public partial class ViewModel_OutsideService_Setup : ViewModel_Shared_Base
{
    private readonly IService_OutsideService _outsideService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInitializePhase))]
    [NotifyPropertyChangedFor(nameof(IsSetupPhase))]
    [NotifyPropertyChangedFor(nameof(PrimaryActionText))]
    [NotifyPropertyChangedFor(nameof(CurrentPhaseText))]
    [NotifyPropertyChangedFor(nameof(PackageSnapshot))]
    private Model_OutsideServiceRequestLine? _currentLine;

    [ObservableProperty]
    private ObservableCollection<Model_OutsideServiceVendorSuggestion> _vendorSuggestions = new();

    [ObservableProperty]
    private Model_OutsideServiceVendorSuggestion? _selectedVendorSuggestion;

    [ObservableProperty]
    private bool _useCustomVendor;

    [ObservableProperty]
    private string _customVendorName = string.Empty;

    [ObservableProperty]
    private string _bolNumber = string.Empty;

    [ObservableProperty]
    private DateTimeOffset _scheduledShipDate = DateTimeOffset.Now;

    [ObservableProperty]
    private string _shippingContact = string.Empty;

    [ObservableProperty]
    private string _setupNotes = string.Empty;

    [ObservableProperty]
    private string _completionNotes = string.Empty;

    public ViewModel_OutsideService_Setup(
        IService_OutsideService outsideService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _outsideService = outsideService ?? throw new ArgumentNullException(nameof(outsideService));
        Title = "Outside Service Setup";
    }

    public event Action? ReturnRequested;

    public event Action? LineSaved;

    /// <summary>
    /// Gets whether the current line is in Initialize.
    /// </summary>
    public bool IsInitializePhase => CurrentLine?.IsInitialize == true;

    /// <summary>
    /// Gets whether the current line is already in Setup.
    /// </summary>
    public bool IsSetupPhase => CurrentLine?.IsSetup == true;

    /// <summary>
    /// Gets the action button text.
    /// </summary>
    public string PrimaryActionText => IsInitializePhase ? "Save Setup" : "Mark Complete";

    /// <summary>
    /// Gets the current phase label.
    /// </summary>
    public string CurrentPhaseText => CurrentLine?.LinePhase.ToString() ?? string.Empty;

    /// <summary>
    /// Gets the multi-line package snapshot.
    /// </summary>
    public string PackageSnapshot => CurrentLine?.PackageSummaryMultiline ?? string.Empty;

    [RelayCommand]
    private void ReturnToQueue()
    {
        ReturnRequested?.Invoke();
    }

    [RelayCommand]
    private async Task SavePrimaryActionAsync()
    {
        if (CurrentLine is null || IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;

            if (CurrentLine.IsInitialize)
            {
                CurrentLine.SetupVendorId = UseCustomVendor
                    ? null
                    : SelectedVendorSuggestion?.VendorId;
                CurrentLine.SetupVendorName = UseCustomVendor
                    ? CustomVendorName.Trim()
                    : SelectedVendorSuggestion?.VendorName;
                CurrentLine.SetupVendorSource = UseCustomVendor ? "custom" : "suggested";
                CurrentLine.BOLNumber = string.IsNullOrWhiteSpace(BolNumber)
                    ? null
                    : BolNumber.Trim();
                CurrentLine.ScheduledShipUtc = ScheduledShipDate.UtcDateTime;
                CurrentLine.ShippingContact = string.IsNullOrWhiteSpace(ShippingContact)
                    ? null
                    : ShippingContact.Trim();
                CurrentLine.SetupNotes = string.IsNullOrWhiteSpace(SetupNotes)
                    ? null
                    : SetupNotes.Trim();

                var setupResult = await _outsideService.SaveSetupAsync(CurrentLine);
                if (!setupResult.IsSuccess)
                {
                    ShowStatus(setupResult.ErrorMessage, InfoBarSeverity.Error);
                    return;
                }

                CurrentLine.LinePhase = Enum_OutsideServiceLinePhase.Setup;
                ShowStatus($"Saved setup for {CurrentLine.QueueKey}.", InfoBarSeverity.Success);
            }
            else
            {
                var completionResult = await _outsideService.MarkCompleteAsync(
                    CurrentLine.OutsideServiceRequestLineId,
                    string.IsNullOrWhiteSpace(CompletionNotes) ? null : CompletionNotes.Trim()
                );
                if (!completionResult.IsSuccess)
                {
                    ShowStatus(completionResult.ErrorMessage, InfoBarSeverity.Error);
                    return;
                }

                CurrentLine.LinePhase = Enum_OutsideServiceLinePhase.Complete;
                CurrentLine.CompletedUtc = DateTime.UtcNow;
                CurrentLine.CompletionNotes = string.IsNullOrWhiteSpace(CompletionNotes)
                    ? null
                    : CompletionNotes.Trim();
                ShowStatus($"Marked {CurrentLine.QueueKey} complete.", InfoBarSeverity.Success);
            }

            OnPropertyChanged(nameof(IsInitializePhase));
            OnPropertyChanged(nameof(IsSetupPhase));
            OnPropertyChanged(nameof(PrimaryActionText));
            OnPropertyChanged(nameof(CurrentPhaseText));
            LineSaved?.Invoke();
            ReturnRequested?.Invoke();
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                nameof(SavePrimaryActionAsync),
                nameof(ViewModel_OutsideService_Setup)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task LoadLineAsync(Model_OutsideServiceRequestLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        CurrentLine = line;
        UseCustomVendor = string.Equals(
            line.SetupVendorSource,
            "custom",
            StringComparison.OrdinalIgnoreCase
        );
        CustomVendorName = UseCustomVendor ? line.SetupVendorName ?? string.Empty : string.Empty;
        BolNumber = line.BOLNumber ?? string.Empty;
        ScheduledShipDate = line.ScheduledShipUtc.HasValue
            ? new DateTimeOffset(line.ScheduledShipUtc.Value)
            : DateTimeOffset.Now;
        ShippingContact = line.ShippingContact ?? string.Empty;
        SetupNotes = line.SetupNotes ?? string.Empty;
        CompletionNotes = line.CompletionNotes ?? string.Empty;

        VendorSuggestions.Clear();
        var suggestions = await _outsideService.GetVendorSuggestionsAsync(line.PartId);
        if (suggestions.IsSuccess && suggestions.Data is not null)
        {
            foreach (var suggestion in suggestions.Data)
            {
                VendorSuggestions.Add(suggestion);
            }
        }

        SelectedVendorSuggestion = VendorSuggestions.FirstOrDefault(suggestion =>
            string.Equals(
                suggestion.VendorId,
                line.SetupVendorId,
                StringComparison.OrdinalIgnoreCase
            )
            || string.Equals(
                suggestion.VendorName,
                line.SetupVendorName,
                StringComparison.OrdinalIgnoreCase
            )
        );

        OnPropertyChanged(nameof(IsInitializePhase));
        OnPropertyChanged(nameof(IsSetupPhase));
        OnPropertyChanged(nameof(PrimaryActionText));
        OnPropertyChanged(nameof(CurrentPhaseText));
        OnPropertyChanged(nameof(PackageSnapshot));
    }
}
