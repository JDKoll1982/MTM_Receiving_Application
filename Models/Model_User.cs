using System;

namespace MTM_Receiving_Application.Models
{
    /// <summary>
    /// Represents a user/employee entity in the authentication system.
    /// Maps to the 'users' database table.
    /// </summary>
    public class Model_User
    {
        // ====================================================================
        // Database Fields
        // ====================================================================
        
        /// <summary>
        /// Unique employee identifier (Primary Key, Auto-increment)
        /// </summary>
        public int EmployeeNumber { get; set; }
        
        /// <summary>
        /// Windows login username (DOMAIN\username format)
        /// Unique constraint in database
        /// </summary>
        public string WindowsUsername { get; set; } = string.Empty;
        
        /// <summary>
        /// Employee full name for display
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        
        /// <summary>
        /// 4-digit numeric PIN for shared terminal login (plain text storage)
        /// Unique constraint in database
        /// </summary>
        public string Pin { get; set; } = string.Empty;
        
        /// <summary>
        /// Department assignment (required)
        /// </summary>
        public string Department { get; set; } = string.Empty;
        
        /// <summary>
        /// Shift assignment: "1st Shift", "2nd Shift", or "3rd Shift"
        /// </summary>
        public string Shift { get; set; } = string.Empty;
        
        /// <summary>
        /// Account active status
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Optional Visual/Infor ERP username (plain text storage)
        /// </summary>
        public string? VisualUsername { get; set; }
        
        /// <summary>
        /// Optional Visual/Infor ERP password (plain text storage, masked in UI)
        /// </summary>
        public string? VisualPassword { get; set; }
        
        /// <summary>
        /// Account creation timestamp
        /// </summary>
        public DateTime CreatedDate { get; set; }
        
        /// <summary>
        /// Windows username of account creator
        /// </summary>
        public string? CreatedBy { get; set; }
        
        /// <summary>
        /// Last modification timestamp
        /// </summary>
        public DateTime ModifiedDate { get; set; }
        
        // ====================================================================
        // Computed Properties
        // ====================================================================
        
        /// <summary>
        /// Display name formatted as "Full Name (Emp #NNNN)"
        /// Used in UI header display
        /// </summary>
        public string DisplayName => $"{FullName} (Emp #{EmployeeNumber})";
        
        /// <summary>
        /// Indicates if user has configured ERP access credentials
        /// </summary>
        public bool HasErpAccess => !string.IsNullOrWhiteSpace(VisualUsername);
        
        // ====================================================================
        // Constructor
        // ====================================================================
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public Model_User()
        {
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }
    }
}
