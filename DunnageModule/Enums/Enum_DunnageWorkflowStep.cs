namespace MTM_Receiving_Application.DunnageModule.Enums
{
    public enum Enum_DunnageWorkflowStep
    {
        ModeSelection = 0,    // Initial step: choose Guided, Manual Entry, or Edit mode
        TypeSelection = 1,    // Select dunnage type (Pallet, Divider, etc.)
        PartSelection = 2,    // Select specific part/spec
        QuantityEntry = 3,    // Enter quantity received
        DetailsEntry = 4,     // Enter additional details (PO number, notes, etc.)
        Review = 5,           // Review session before saving
        ManualEntry = 6,      // Bulk grid entry mode
        EditMode = 7          // Historical data editing mode
    }
}
