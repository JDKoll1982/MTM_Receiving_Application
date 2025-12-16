namespace MTM_Receiving_Application.Models.Enums;

/// <summary>
/// Identifies the type of label being generated
/// </summary>
public enum Enum_LabelType
{
    /// <summary>
    /// Standard receiving label
    /// </summary>
    Receiving = 1,

    /// <summary>
    /// Dunnage/packing material label
    /// </summary>
    Dunnage = 2,

    /// <summary>
    /// UPS or FedEx shipping label
    /// </summary>
    UPSFedEx = 3,

    /// <summary>
    /// Mini receiving label variant
    /// </summary>
    MiniReceiving = 4,

    /// <summary>
    /// Mini coil label variant
    /// </summary>
    MiniCoil = 5
}
