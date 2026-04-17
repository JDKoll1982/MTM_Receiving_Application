using System;

namespace MTM_Receiving_Application.Module_Core.Models.Systems;

/// <summary>
/// Represents the globally required application version stored in MySQL.
/// </summary>
public class Model_SoftwareVersion
{
    public int Id { get; set; }

    public string RequiredVersion { get; set; } = string.Empty;

    public string UpdatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
