using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_OutsideService.ViewModels;

namespace MTM_Receiving_Application.Module_OutsideService.Views;

/// <summary>
/// Completed-history page for Outside Service lines.
/// </summary>
public sealed partial class View_OutsideService_CompleteHistory : Page
{
    public ViewModel_OutsideService_CompleteHistory ViewModel { get; }

    public View_OutsideService_CompleteHistory()
    {
        ViewModel = App.GetService<ViewModel_OutsideService_CompleteHistory>();
        InitializeComponent();
        Loaded += async (_, _) => await ViewModel.LoadCommand.ExecuteAsync(null);
    }
}
