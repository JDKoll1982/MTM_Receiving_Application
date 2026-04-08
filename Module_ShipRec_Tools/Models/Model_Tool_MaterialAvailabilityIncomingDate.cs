using System;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// UI model for one distinct incoming-material date shown on a material availability card.
/// </summary>
public class Model_Tool_MaterialAvailabilityIncomingDate
{
    public string Label { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public string DisplayText => $"{Label} {Date:MM/dd/yyyy}";
}
