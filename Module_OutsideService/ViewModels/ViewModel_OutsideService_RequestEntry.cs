using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
/// Request-entry ViewModel for the Outside Service module.
/// </summary>
public partial class ViewModel_OutsideService_RequestEntry : ViewModel_Shared_Base
{
    private readonly IService_OutsideService _outsideService;
    private readonly IService_UserSessionManager _sessionManager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedLine))]
    private Model_OutsideServiceRequestLine? _selectedLine;

    [ObservableProperty]
    private string _requestNotes = string.Empty;

    [ObservableProperty]
    private string _createdByDisplay = string.Empty;

    [ObservableProperty]
    private DateTimeOffset _requestDate = DateTimeOffset.Now;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DraftBadgeText))]
    [NotifyPropertyChangedFor(nameof(HasLines))]
    private ObservableCollection<Model_OutsideServiceRequestLine> _currentLines = new();

    public ViewModel_OutsideService_RequestEntry(
        IService_OutsideService outsideService,
        IService_UserSessionManager sessionManager,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _outsideService = outsideService ?? throw new ArgumentNullException(nameof(outsideService));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));

        var user = _sessionManager.CurrentSession?.User;
        CreatedByDisplay = user?.DisplayName ?? Environment.UserName;
        CurrentLines.CollectionChanged += OnCurrentLinesChanged;
        Title = "Outside Service Request Entry";
    }

    public event Action? AddLineRequested;

    public event Action<Model_OutsideServiceRequest>? RequestSaved;

    /// <summary>
    /// Gets whether at least one draft line exists.
    /// </summary>
    public bool HasLines => CurrentLines.Count > 0;

    /// <summary>
    /// Gets the draft line count badge text.
    /// </summary>
    public string DraftBadgeText =>
        CurrentLines.Count == 1 ? "1 line item" : $"{CurrentLines.Count} line items";

    /// <summary>
    /// Gets whether a line is selected.
    /// </summary>
    public bool HasSelectedLine => SelectedLine is not null;

    [RelayCommand]
    private void RequestAddLine()
    {
        AddLineRequested?.Invoke();
    }

    [RelayCommand]
    private void ClearRequest()
    {
        CurrentLines.Clear();
        SelectedLine = null;
        RequestNotes = string.Empty;
        RequestDate = DateTimeOffset.Now;
        ShowStatus("Draft request cleared.", InfoBarSeverity.Informational);
    }

    [RelayCommand]
    private async Task SaveRequestAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (CurrentLines.Count == 0)
        {
            ShowStatus("Add at least one line before saving the request.", InfoBarSeverity.Warning);
            return;
        }

        try
        {
            IsBusy = true;
            ShowStatus("Saving Outside Service request…");

            var currentUser = _sessionManager.CurrentSession?.User;
            var request = new Model_OutsideServiceRequest
            {
                CreatedByUser = currentUser?.WindowsUsername ?? Environment.UserName,
                CreatedByDisplay = currentUser?.DisplayName ?? CreatedByDisplay,
                CreatedUtc = DateTime.UtcNow,
                RequestNotes = string.IsNullOrWhiteSpace(RequestNotes) ? null : RequestNotes.Trim(),
                Lines = CurrentLines.Select(CloneLine).ToList(),
            };

            var result = await _outsideService.CreateRequestAsync(request);
            if (!result.IsSuccess || result.Data is null)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Error);
                return;
            }

            ShowStatus($"Saved request {result.Data.RequestNumber}.", InfoBarSeverity.Success);
            RequestSaved?.Invoke(result.Data);
            ClearRequest();
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                nameof(SaveRequestAsync),
                nameof(ViewModel_OutsideService_RequestEntry)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void AddDraftLine(Model_OutsideServiceRequestLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        line.LineNumber = CurrentLines.Count + 1;
        line.CreatedByDisplay = CreatedByDisplay;
        CurrentLines.Add(line);
        SelectedLine = line;
        ShowStatus(
            $"Added line {line.LineNumber} for part {line.PartId}.",
            InfoBarSeverity.Success
        );
    }

    public async Task<bool> ValidatePartAsync(string partId)
    {
        var result = await _outsideService.ValidatePartAsync(partId);
        return result.IsSuccess && result.Data;
    }

    public async Task<Model_OutsideServicePartMatchSuggestion[]> GetPartSuggestionsAsync(
        string partId
    )
    {
        var result = await _outsideService.GetPartSuggestionsAsync(partId);
        return result.IsSuccess && result.Data is not null
            ? result.Data.ToArray()
            : Array.Empty<Model_OutsideServicePartMatchSuggestion>();
    }

    private void OnCurrentLinesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(DraftBadgeText));
        OnPropertyChanged(nameof(HasLines));
    }

    private static Model_OutsideServiceRequestLine CloneLine(Model_OutsideServiceRequestLine source)
    {
        return new Model_OutsideServiceRequestLine
        {
            LineNumber = source.LineNumber,
            PartId = source.PartId,
            PackageCount = source.PackageCount,
            Packages = source.Packages.ConvertAll(package => new Model_OutsideServiceRequestPackage
            {
                PackageSequence = package.PackageSequence,
                PackageQuantity = package.PackageQuantity,
            }),
        };
    }
}
