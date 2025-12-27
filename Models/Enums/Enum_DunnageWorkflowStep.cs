namespace MTM_Receiving_Application.Models.Enums
{
    public enum Enum_DunnageWorkflowStep
    {
        ModeSelection = 0,    // Initial step: choose Receiving or Inventory mode
        TypeSelection = 1,    // Select dunnage type (Pallet, Divider, etc.)
        PartSelection = 2,    // Select specific part/spec
        QuantityEntry = 3,    // Enter quantity received
        DetailsEntry = 4,     // Enter additional details (PO number, notes, etc.)
        Review = 5            // Review session before saving
    }
}
