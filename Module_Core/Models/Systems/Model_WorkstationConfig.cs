using System;

namespace MTM_Receiving_Application.Module_Core.Models.Systems
{
    /// <summary>
    /// Represents workstation detection and configuration logic.
    /// Maps to 'workstation_config' database table.
    /// </summary>
    public class Model_WorkstationConfig
    {
        // ====================================================================
        // Workstation Information
        // ====================================================================
        
        /// <summary>
        /// Windows computer name (from Environment.MachineName)
        /// </summary>
        public string ComputerName { get; set; } = string.Empty;
        
        /// <summary>
        /// Workstation type: "personal_workstation" or "shared_terminal"
        /// </summary>
        public string WorkstationType { get; set; } = string.Empty;
        
        /// <summary>
        /// Optional description of workstation location/purpose
        /// </summary>
        public string? Description { get; set; }
        
        // ====================================================================
        // Computed Properties
        // ====================================================================
        
        /// <summary>
        /// True if workstation is configured as shared terminal (PIN login required)
        /// </summary>
        public bool IsSharedTerminal => WorkstationType == "shared_terminal";
        
        /// <summary>
        /// True if workstation is personal workstation (automatic Windows login)
        /// </summary>
        public bool IsPersonalWorkstation => WorkstationType == "personal_workstation";
        
        /// <summary>
        /// Session timeout duration based on workstation type
        /// 15 minutes for shared terminals, 30 minutes for personal workstations
        /// </summary>
        public TimeSpan TimeoutDuration => IsSharedTerminal 
            ? TimeSpan.FromMinutes(15) 
            : TimeSpan.FromMinutes(30);
        
        // ====================================================================
        // Constructor
        // ====================================================================
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public Model_WorkstationConfig()
        {
        }
        
        /// <summary>
        /// Constructor with computer name detection
        /// </summary>
        /// <param name="computerName">Computer name (defaults to Environment.MachineName)</param>
        public Model_WorkstationConfig(string? computerName = null)
        {
            ComputerName = computerName ?? Environment.MachineName;
        }
    }
}

