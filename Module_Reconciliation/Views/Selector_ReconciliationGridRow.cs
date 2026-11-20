using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Views;

/// <summary>
/// Picks the row template for the day grid, which lists a group's header and its lines
/// in one flat source so a group can be folded shut.
/// </summary>
public sealed partial class Selector_ReconciliationGridRow : DataTemplateSelector
{
    /// <summary>The template for a part-and-date group header.</summary>
    public DataTemplate? GroupHeaderTemplate { get; set; }

    /// <summary>The template for one receiving line.</summary>
    public DataTemplate? FindingTemplate { get; set; }

    /// <inheritdoc />
    protected override DataTemplate? SelectTemplateCore(object item) =>
        item is Model_ReconciliationFindingGroup ? GroupHeaderTemplate : FindingTemplate;

    /// <inheritdoc />
    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container) =>
        SelectTemplateCore(item);
}
