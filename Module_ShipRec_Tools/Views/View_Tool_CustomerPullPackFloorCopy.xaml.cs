using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Floor-copy style print preview page for Customer Pull n' Pack demand output.
/// </summary>
public sealed partial class View_Tool_CustomerPullPackFloorCopy : Page
{
    public Model_CustomerPullPack_PrintContext PrintContext { get; }

    public string GeneratedAtDisplay => PrintContext.GeneratedAt.ToLocalTime().ToString("g");

    public View_Tool_CustomerPullPackFloorCopy(Model_CustomerPullPack_PrintContext printContext)
    {
        ArgumentNullException.ThrowIfNull(printContext);
        PrintContext = printContext;
        InitializeComponent();
    }
}
