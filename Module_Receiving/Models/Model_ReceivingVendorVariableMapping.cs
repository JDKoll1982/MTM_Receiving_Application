using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Receiving.Models;

public partial class Model_ReceivingVendorVariableMapping : ObservableObject
{
    [ObservableProperty]
    private string _vendorName = string.Empty;

    [ObservableProperty]
    private string _variableName = string.Empty;

    public string DisplayName => $"{VendorName} -> {VariableName}";

    public bool MatchesVendorName(string? vendorName)
    {
        if (string.IsNullOrWhiteSpace(VendorName) || string.IsNullOrWhiteSpace(vendorName))
        {
            return false;
        }

        return vendorName.Contains(VendorName.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
