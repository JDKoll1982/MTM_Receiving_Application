using System;

namespace MTM_Receiving_Application.ReceivingModule.Models
{
    /// <summary>
    /// Stores user's preferred package type for specific part IDs.
    /// Persisted to MySQL database for use across sessions.
    /// </summary>
    public class Model_PackageTypePreference
    {
        public int PreferenceID { get; set; }
        
        public string PartID { get; set; } = string.Empty;
        
        public string PackageTypeName { get; set; } = string.Empty;
        
        public string? CustomTypeName { get; set; }
        
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }
}
