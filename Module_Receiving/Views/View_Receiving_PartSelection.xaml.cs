using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views;

/// <summary>
/// The guided "Select part" step. Everything it shows comes from
/// <see cref="ViewModel_Receiving_PartSelection"/>; the code here only moves focus.
/// </summary>
public sealed partial class View_Receiving_PartSelection : UserControl, IReceivingWorkflowFocusable
{
    private readonly IService_Focus _focusService;

    public View_Receiving_PartSelection(
        ViewModel_Receiving_PartSelection viewModel,
        IService_Focus focusService
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(focusService);

        ViewModel = viewModel;
        _focusService = focusService;
        InitializeComponent();
    }

    public ViewModel_Receiving_PartSelection ViewModel { get; }

    /// <summary>
    /// Puts the cursor in the search box, so a scan gun's next trigger fills it and does not have to
    /// be aimed at the right field first.
    /// </summary>
    public bool FocusForAccess() => _focusService.TrySetFocus(PartSearchBox);
}
