using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.ReceivingModule.Models;

public partial class Model_UserPreference : ObservableObject
{
    /// <summary>Unique identifier (auto-increment)</summary>
    [ObservableProperty]
    private int _preferenceId;

    /// <summary>Username (Windows username or PIN-authenticated user)</summary>
    [ObservableProperty]
    private string _username = string.Empty;

    /// <summary>Preferred package type: "Package" or "Pallet"</summary>
    [ObservableProperty]
    private string _preferredPackageType = "Package";

    /// <summary>Timestamp when preference was last updated</summary>
    [ObservableProperty]
    private DateTime _lastUpdated;

    /// <summary>Workstation identifier (for shared terminals)</summary>
    [ObservableProperty]
    private string _workstation = string.Empty;
}
