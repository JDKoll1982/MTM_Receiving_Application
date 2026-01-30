using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Step1;

namespace MTM_Receiving_Application.Module_Receiving.Views.Wizard.Step1;

/// <summary>
/// Container page for Step 1: Order &amp; Part Selection.
/// Hosts all Step 1 user controls (PO Number, Part Selection, Load Count).
/// ENHANCED: Wires PONumberEntry parts list to PartSelection for dropdown population.
/// ENHANCED: Wires all child ViewModels to Step1Summary for real-time updates.
/// </summary>
public sealed partial class View_Receiving_Wizard_Display_Step1Container : Page
{
    public View_Receiving_Wizard_Display_Step1Container()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Wire up parts list transfer from PONumberEntry to PartSelection
        if (PONumberEntry.DataContext is ViewModel_Receiving_Wizard_Display_PONumberEntry poViewModel &&
            PartSelection.DataContext is ViewModel_Receiving_Wizard_Display_PartSelection partViewModel)
        {
            // Subscribe to PO ViewModel's parts collection changes
            poViewModel.AvailablePartsOnPo.CollectionChanged += (s, args) =>
            {
                // When PO loads parts, transfer them to PartSelection
                partViewModel.SetAvailablePartsFromPo(poViewModel.AvailablePartsOnPo);
            };

            // Wire up Step1Summary updates
            if (Step1Summary.DataContext is ViewModel_Receiving_Wizard_Display_Step1Summary summaryViewModel &&
                LoadCountEntry.DataContext is ViewModel_Receiving_Wizard_Display_LoadCountEntry loadCountViewModel)
            {
                // Subscribe to PO Number changes
                poViewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(poViewModel.PoNumber) ||
                        args.PropertyName == nameof(poViewModel.IsNonPo))
                    {
                        UpdateSummary(summaryViewModel, poViewModel, partViewModel, loadCountViewModel);
                    }
                };

                // Subscribe to Part selection changes
                partViewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(partViewModel.SelectedPartFromPo))
                    {
                        UpdateSummary(summaryViewModel, poViewModel, partViewModel, loadCountViewModel);
                    }
                };

                // Subscribe to Load Count changes
                loadCountViewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(loadCountViewModel.LoadCount))
                    {
                        UpdateSummary(summaryViewModel, poViewModel, partViewModel, loadCountViewModel);
                    }
                };

                // Initial update
                UpdateSummary(summaryViewModel, poViewModel, partViewModel, loadCountViewModel);
            }
        }
    }

    private void UpdateSummary(
        ViewModel_Receiving_Wizard_Display_Step1Summary summaryViewModel,
        ViewModel_Receiving_Wizard_Display_PONumberEntry poViewModel,
        ViewModel_Receiving_Wizard_Display_PartSelection partViewModel,
        ViewModel_Receiving_Wizard_Display_LoadCountEntry loadCountViewModel)
    {
        var partNumber = partViewModel.SelectedPartFromPo?.PartNumber ?? string.Empty;
        summaryViewModel.UpdateSummary(
            poViewModel.PoNumber,
            partNumber,
            loadCountViewModel.LoadCount,
            poViewModel.IsNonPo);
    }
}
