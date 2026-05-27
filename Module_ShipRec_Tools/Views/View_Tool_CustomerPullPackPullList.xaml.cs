using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Pull-list style print preview page for Customer Pull n' Pack queue output.
/// </summary>
public sealed partial class View_Tool_CustomerPullPackPullList : Page
{
    public Model_CustomerPullPack_PrintContext PrintContext { get; }

    public View_Tool_CustomerPullPackPullList(Model_CustomerPullPack_PrintContext printContext)
    {
        ArgumentNullException.ThrowIfNull(printContext);
        PrintContext = printContext;
        InitializeComponent();
    }
}
