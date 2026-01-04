namespace MTM_Receiving_Application.Module_Core.Models.Enums
{
    /// <summary>
    /// Workflow steps enumeration for Receiving.
    /// </summary>
    public enum Enum_ReceivingWorkflowStep
    {
        ModeSelection = 0,
        ManualEntry = 1,
        EditMode = 2,
        POEntry = 3,
        PartSelection = 4,
        LoadEntry = 5,
        WeightQuantityEntry = 6,
        HeatLotEntry = 7,
        PackageTypeEntry = 8,
        Review = 9,
        Saving = 10,
        Complete = 11
    }
}

