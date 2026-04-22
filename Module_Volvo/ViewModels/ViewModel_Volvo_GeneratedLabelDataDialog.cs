using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;

namespace MTM_Receiving_Application.Module_Volvo.ViewModels;

/// <summary>
/// ViewModel for reviewing and clearing the active Volvo generated-label queue.
/// </summary>
public partial class ViewModel_Volvo_GeneratedLabelDataDialog : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private ObservableCollection<Model_VolvoGeneratedLabelData> _rows = new();

    [ObservableProperty]
    private string _summaryText = string.Empty;

    public bool HasRows => Rows.Count > 0;

    public ViewModel_Volvo_GeneratedLabelDataDialog(
        IMediator mediator,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading generated label data...";

            var result = await _mediator.Send(new GetVolvoGeneratedLabelDataQuery());
            if (!result.IsSuccess || result.Data == null)
            {
                Rows = new ObservableCollection<Model_VolvoGeneratedLabelData>();
                SummaryText = result.ErrorMessage ?? "No generated label data is available.";
                OnPropertyChanged(nameof(HasRows));
                return;
            }

            Rows = new ObservableCollection<Model_VolvoGeneratedLabelData>(result.Data);
            SummaryText =
                $"{Rows.Count} generated label row{(Rows.Count == 1 ? string.Empty : "s")}";
            StatusMessage = SummaryText;
            OnPropertyChanged(nameof(HasRows));
        }
        catch (Exception ex)
        {
            Rows = new ObservableCollection<Model_VolvoGeneratedLabelData>();
            SummaryText = "Failed to load generated label data.";
            StatusMessage = SummaryText;
            OnPropertyChanged(nameof(HasRows));
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadAsync),
                nameof(ViewModel_Volvo_GeneratedLabelDataDialog)
            );
        }
        finally
        {
            IsBusy = false;
            ClearLabelDataCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanClearLabelData))]
    private async Task ClearLabelDataAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Clearing generated label data...";

            var result = await _mediator.Send(
                new ClearLabelDataCommand { ArchivedBy = Environment.UserName }
            );
            if (!result.IsSuccess)
            {
                await _errorHandler.ShowUserErrorAsync(
                    result.ErrorMessage ?? "Failed to clear generated label data.",
                    "Generated Label Data",
                    nameof(ClearLabelDataAsync)
                );
                return;
            }

            await LoadAsync();
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(ClearLabelDataAsync),
                nameof(ViewModel_Volvo_GeneratedLabelDataDialog)
            );
        }
        finally
        {
            IsBusy = false;
            ClearLabelDataCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanClearLabelData()
    {
        return !IsBusy && HasRows;
    }
}
