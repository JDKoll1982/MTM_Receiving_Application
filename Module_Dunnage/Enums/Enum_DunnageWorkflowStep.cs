namespace MTM_Receiving_Application.Module_Dunnage.Enums
{
    public enum Enum_DunnageWorkflowStep
    {
        TypeSelection = 0, // Select dunnage type (Pallet, Divider, etc.)
        PartSelection = 1, // Select specific part/spec
        QuantityEntry = 2, // Enter quantity received
        DetailsEntry = 3, // Enter additional details (PO number, notes, etc.)
        Review = 4, // Review session before saving
        EditMode = 5, // Historical data editing mode
        ImagePartSearch = 6, // Browse image-backed parts and hand off into part selection
    }
}
