namespace MTM_Receiving_Application.Module_Volvo.Models;

/// <summary>
/// Represents a component relationship for Volvo parts (component explosion)
/// Maps to volvo_part_components database table
/// Example: V-EMB-500 includes 1× V-EMB-2 and 1× V-EMB-92
/// </summary>
public class Model_VolvoPartComponent
{
    /// <summary>
    /// Auto-increment primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The parent part number (e.g., V-EMB-500)
    /// </summary>
    public string ParentPartNumber { get; set; } = string.Empty;

    /// <summary>
    /// The component part number included with the parent (e.g., V-EMB-2)
    /// </summary>
    public string ComponentPartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Quantity of this component included per skid of the parent part
    /// Usually 1, but can be higher (e.g., V-EMB-780 includes 3× V-EMB-26)
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Quantity per skid for the component part (from volvo_parts_master)
    /// Used for calculating total piece count during component explosion
    /// </summary>
    public int ComponentQuantityPerSkid { get; set; }
}
