using System;
using System.Collections.ObjectModel;

namespace Module_UI_Mockup.Models;

/// <summary>
/// Sample data model providing realistic manufacturing data for UI demonstrations.
/// All data is static and requires no database or external service.
/// </summary>
public static class Model_UI_SampleData
{
    #region Purchase Orders
    /// <summary>
    /// Gets sample purchase order numbers.
    /// </summary>
    public static ObservableCollection<string> PurchaseOrders { get; } = new()
    {
        "PO-123456", "PO-066868", "PO-789012", "PO-345678", "PO-901234"
    };
    #endregion

    #region Part Numbers
    /// <summary>
    /// Gets sample part numbers following manufacturing naming conventions.
    /// </summary>
    public static ObservableCollection<string> PartNumbers { get; } = new()
    {
        "ABC-12345-XYZ", "MCC-45678-A", "MMF-98765-B", "PRT-11223-C", "ASM-55667-D",
        "BRK-99887-E", "MTL-77665-F", "PLT-44332-G", "CMP-22110-H", "SUB-88776-I"
    };
    #endregion

    #region Employees
    /// <summary>
    /// Gets sample employee data with names, numbers, and roles.
    /// </summary>
    public static ObservableCollection<EmployeeData> Employees { get; } = new()
    {
        new EmployeeData { Name = "John Doe", EmployeeNumber = "1001", Role = "Operator", Initials = "JD" },
        new EmployeeData { Name = "Jane Smith", EmployeeNumber = "1002", Role = "Lead", Initials = "JS" },
        new EmployeeData { Name = "Mike Johnson", EmployeeNumber = "1003", Role = "Material Handler", Initials = "MJ" },
        new EmployeeData { Name = "Sarah Williams", EmployeeNumber = "1004", Role = "Quality", Initials = "SW" },
        new EmployeeData { Name = "David Brown", EmployeeNumber = "1005", Role = "Operator", Initials = "DB" }
    };
    #endregion

    #region Transactions
    /// <summary>
    /// Gets sample receiving transaction data.
    /// </summary>
    public static ObservableCollection<TransactionData> Transactions { get; } = new()
    {
        new TransactionData { Id = 1, Date = DateTime.Now.AddDays(-2), Quantity = 100, Status = "Completed", Receiver = "John Doe" },
        new TransactionData { Id = 2, Date = DateTime.Now.AddDays(-1), Quantity = 50, Status = "In Progress", Receiver = "Jane Smith" },
        new TransactionData { Id = 3, Date = DateTime.Now, Quantity = 75, Status = "Pending", Receiver = "Mike Johnson" },
        new TransactionData { Id = 4, Date = DateTime.Now.AddHours(-3), Quantity = 200, Status = "Completed", Receiver = "Sarah Williams" },
        new TransactionData { Id = 5, Date = DateTime.Now.AddHours(-1), Quantity = 150, Status = "In Progress", Receiver = "David Brown" }
    };
    #endregion

    #region Dunnage Types
    /// <summary>
    /// Gets sample dunnage types (packaging materials).
    /// </summary>
    public static ObservableCollection<string> DunnageTypes { get; } = new()
    {
        "Box 12x8x6", "Box 18x12x10", "Pallet 48x40 Standard", "Pallet 48x48 Euro",
        "Tote 24x18x12", "Gaylord 48x40x36", "Skid 40x48", "Crate 36x24x18"
    };
    #endregion

    #region Routing Recipients
    /// <summary>
    /// Gets sample routing recipients (departments and stations).
    /// </summary>
    public static ObservableCollection<RoutingRecipient> RoutingRecipients { get; } = new()
    {
        new RoutingRecipient { Department = "Press Floor", Station = "Press 1", Location = "Building A" },
        new RoutingRecipient { Department = "Press Floor", Station = "Press 2", Location = "Building A" },
        new RoutingRecipient { Department = "Assembly", Station = "Line A", Location = "Building B" },
        new RoutingRecipient { Department = "Assembly", Station = "Line B", Location = "Building B" },
        new RoutingRecipient { Department = "Shipping", Station = "Dock 1", Location = "Building C" }
    };
    #endregion

    #region Helper Methods
    /// <summary>
    /// Gets a placeholder image URI for the specified image name.
    /// Includes fallback pattern for missing images.
    /// </summary>
    /// <param name="imageName">Name of the image file.</param>
    /// <returns>URI to the image or placeholder.</returns>
    public static string GetImageUri(string imageName)
    {
        return $"/Assets/{imageName}";
    }

    /// <summary>
    /// Gets employee photo URI using DiceBear API for placeholder avatars.
    /// </summary>
    /// <param name="employeeNumber">The employee number.</param>
    /// <returns>URI to employee photo or generated avatar.</returns>
    public static string GetEmployeePhoto(string employeeNumber)
    {
        return $"https://api.dicebear.com/7.x/initials/svg?seed={employeeNumber}";
    }
    #endregion
}

#region Data Models
/// <summary>
/// Represents employee information.
/// </summary>
public class EmployeeData
{
    public string Name { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
}

/// <summary>
/// Represents a receiving transaction.
/// </summary>
public class TransactionData
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Receiver { get; set; } = string.Empty;
}

/// <summary>
/// Represents a routing recipient (department/station).
/// </summary>
public class RoutingRecipient
{
    public string Department { get; set; } = string.Empty;
    public string Station { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}
#endregion
