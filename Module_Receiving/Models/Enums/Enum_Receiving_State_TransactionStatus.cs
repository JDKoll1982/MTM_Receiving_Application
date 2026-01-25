namespace MTM_Receiving_Application.Module_Receiving.Models.Enums;

/// <summary>
/// Defines the status of a receiving transaction
/// </summary>
public enum Enum_Receiving_State_TransactionStatus
{
    /// <summary>
    /// Transaction in progress (not saved yet)
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Transaction saved to database and CSV
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Transaction was modified in Edit Mode
    /// </summary>
    Modified = 3,

    /// <summary>
    /// Transaction cancelled before save
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Transaction failed to save
    /// </summary>
    Failed = 5
}
